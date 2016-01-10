using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;



namespace DXAppProto2.FilterExpressions {


public class Parser 
{
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _intCon = 2;
	public const int _realCon = 3;
	public const int _charCon = 4;
	public const int _stringCon = 5;
	public const int maxT = 47;


	private const bool _T = true;
	private const bool _x = false;
	private const int minErrDist = 2;
	
	private Scanner scanner;
	private Errors  errors;

	private Token t;    // last recognized token
	private Token la;   // lookahead token
	private int errDist = minErrDist;

	public FilterExpressionNode Root { get; private set; }
	
private FilterExpressionBinaryOperator ResolveOperator(string oper)
{
	switch(oper)
	{
		case "And": return FilterExpressionBinaryOperator.And;
		case "Xor": return FilterExpressionBinaryOperator.Xor;
		case "+": return FilterExpressionBinaryOperator.Add;
		case "-": return FilterExpressionBinaryOperator.Subtract;
		case "*": return FilterExpressionBinaryOperator.Multiply;
		case "/": return FilterExpressionBinaryOperator.Divide;
		case "%": return FilterExpressionBinaryOperator.Remainder;
		case "=": return FilterExpressionBinaryOperator.Equals;
		case "<>": return FilterExpressionBinaryOperator.NotEquals;
		case "<": return FilterExpressionBinaryOperator.LessThan;
		case ">": return FilterExpressionBinaryOperator.GreatThan;
		case ">=": return FilterExpressionBinaryOperator.GreatThanOrEquals;
		case "<=": return FilterExpressionBinaryOperator.LessThanOrEquals;		
	}

	throw new NotSupportedException();
}

private string Unescape(string txt)
{
    if (string.IsNullOrEmpty(txt)) { return txt; }
    StringBuilder retval = new StringBuilder(txt.Length);
    for (int ix = 0; ix < txt.Length; )
    {
        int jx = txt.IndexOf('\\', ix);
        if (jx < 0 || jx == txt.Length - 1) jx = txt.Length;
        retval.Append(txt, ix, jx - ix);
        if (jx >= txt.Length) break;
        switch (txt[jx + 1])
        {
            case 'n': retval.Append('\n'); break;  // Line feed
            case 'r': retval.Append('\r'); break;  // Carriage return
            case 't': retval.Append('\t'); break;  // Tab
            case '\\': retval.Append('\\'); break; // Don't escape
            default:                                 // Unrecognized, copy as-is
                retval.Append('\\').Append(txt[jx + 1]); break;
        }
        ix = jx + 2;
    }
    return retval.ToString();
}

private string MakeString(string v)
{
	var unquoted = v.Substring(1, v.Length-2);
	var unescaped = Unescape(unquoted);
	return unescaped;
}

private void MakeReal(string v, out object val, out Type type)
{
	if (v.EndsWith("d", StringComparison.InvariantCultureIgnoreCase))
	{
		val = double.Parse(v.Substring(0, v.Length-1));
		type= typeof(double);
	}
	else if (v.EndsWith("f", StringComparison.InvariantCultureIgnoreCase))
	{
		val = float.Parse(v.Substring(0, v.Length-1));
		type= typeof(float);
	}
	else if (v.EndsWith("m", StringComparison.InvariantCultureIgnoreCase))
	{
		val = decimal.Parse(v.Substring(0, v.Length - 1));
		type = typeof (decimal);
	}
	else
	{
		val = double.Parse(v);
		type= typeof(double);	
	}
}

private void MakeInteger(string v, out object val, out Type type)
{
	if (v.EndsWith("u", StringComparison.InvariantCultureIgnoreCase))
	{
		val = uint.Parse(v.Substring(0, v.Length-1));
		type= typeof(uint);
	}
	else if(v.EndsWith("ul", StringComparison.InvariantCultureIgnoreCase)
		|| v.EndsWith("lu", StringComparison.InvariantCultureIgnoreCase))
	{
		val = ulong.Parse(v.Substring(0, v.Length-2));
		type= typeof(ulong);
	}
	else if (v.EndsWith("l", StringComparison.InvariantCultureIgnoreCase))
	{
		val = long.Parse(v.Substring(0, v.Length-1));
		type= typeof(long);
	}
	else
	{
		val = int.Parse(v);
		type= typeof(int);	
	}
}

private Type MakeNullable(Type t)
{
	var nullable = typeof (Nullable<>);
	var nullableConcrete = nullable.MakeGenericType(t);
	return nullableConcrete;
}



	public Parser(Scanner scanner) 
	{
		this.scanner = scanner;
		errors = new Errors();
	}

	private void SynErr (int n) 
	{
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	private void SemErr (string msg) 
	{
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	private void Get () 
	{
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	private void Expect (int n) 
	{
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	private bool StartOf (int s) 
	{
		return set[s, la.kind];
	}
	
	private void ExpectWeak (int n, int follow) 
	{
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}

	private bool WeakSeparator(int n, int syFol, int repFol) 
	{
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}
	
	void C() {
		FilterExpressionNode n; 
		Expression(out n);
		Root = n; 
	}

	void Expression(out FilterExpressionNode node) {
		ExpressionOr(out node);
	}

	void ExpressionOr(out FilterExpressionNode node) {
		FilterExpressionNode n, m; 
		ExpressionAnd(out n);
		while (la.kind == 6) {
			Get();
			ExpressionAnd(out m);
			n = new FilterExpressionBinaryNode(FilterExpressionBinaryOperator.Or, n, m); 
		}
		node = n; 
	}

	void ExpressionAnd(out FilterExpressionNode node) {
		FilterExpressionNode n, m; string o; 
		ExpressionSum(out n);
		while (la.kind == 7 || la.kind == 8) {
			if (la.kind == 7) {
				Get();
			} else {
				Get();
			}
			o = t.val; 
			ExpressionSum(out m);
			n = new FilterExpressionBinaryNode(ResolveOperator(o), n, m); 
		}
		node = n; 
	}

	void ExpressionSum(out FilterExpressionNode node) {
		FilterExpressionNode n, m; string o; 
		ExpressionMult(out n);
		while (la.kind == 9 || la.kind == 10) {
			if (la.kind == 9) {
				Get();
			} else {
				Get();
			}
			o = t.val; 
			ExpressionMult(out m);
			n = new FilterExpressionBinaryNode(ResolveOperator(o), n, m); 
		}
		node = n; 
	}

	void ExpressionMult(out FilterExpressionNode node) {
		FilterExpressionNode n, m; string o; 
		ExpressionEqu(out n);
		while (la.kind == 11 || la.kind == 12 || la.kind == 13) {
			if (la.kind == 11) {
				Get();
			} else if (la.kind == 12) {
				Get();
			} else {
				Get();
			}
			o = t.val; 
			ExpressionEqu(out m);
			n = new FilterExpressionBinaryNode(ResolveOperator(o), n, m); 
		}
		node = n; 
	}

	void ExpressionEqu(out FilterExpressionNode node) {
		FilterExpressionNode n, m; string o; 
		ExpressionRel(out n);
		while (la.kind == 14 || la.kind == 15) {
			if (la.kind == 14) {
				Get();
			} else {
				Get();
			}
			o = t.val; 
			ExpressionRel(out m);
			n = new FilterExpressionBinaryNode(ResolveOperator(o), n, m); 
		}
		node = n; 
	}

	void ExpressionRel(out FilterExpressionNode node) {
		FilterExpressionNode n, m; string o; 
		ExpressionUnary(out n);
		while (StartOf(1)) {
			if (la.kind == 16) {
				Get();
			} else if (la.kind == 17) {
				Get();
			} else if (la.kind == 18) {
				Get();
			} else {
				Get();
			}
			o = t.val; 
			ExpressionUnary(out m);
			n = new FilterExpressionBinaryNode(ResolveOperator(o), n, m); 
		}
		node = n; 
	}

	void ExpressionUnary(out FilterExpressionNode node) {
		FilterExpressionNode n; FilterExpressionUnaryOperator? oper = null; 
		if (la.kind == 10 || la.kind == 20) {
			if (la.kind == 10) {
				Get();
				oper = FilterExpressionUnaryOperator.Complement; 
			} else {
				Get();
				oper = FilterExpressionUnaryOperator.Not; 
			}
		}
		ExpressionTerminal(out n);
		node = oper.HasValue ? new FilterExpressionUnaryNode(oper.Value, n) : n; 
	}

	void ExpressionTerminal(out FilterExpressionNode node) {
		node = null; 
		if (StartOf(2)) {
			Literal(out node);
		} else if (la.kind == 1) {
			FieldReferenceOrMethodCall(out node);
		} else if (la.kind == 21) {
			Get();
			if (StartOf(3)) {
				Cast(out node);
			} else if (StartOf(4)) {
				Expression(out node);
				Expect(22);
			} else SynErr(48);
		} else SynErr(49);
	}

	void Literal(out FilterExpressionNode node) {
		string mu = string.Empty; node = null; 
		switch (la.kind) {
		case 2: {
			Get();
			object v; Type ty; MakeInteger(t.val, out v, out ty); 
			if (la.kind == 1) {
				MeasurementUnit(out mu);
			}
			node = new FilterExpressionLiteralNode(ty, v, mu); 
			break;
		}
		case 3: {
			Get();
			object v; Type ty; MakeReal(t.val, out v, out ty); 
			if (la.kind == 1) {
				MeasurementUnit(out mu);
			}
			node = new FilterExpressionLiteralNode(ty, v, mu); 
			break;
		}
		case 4: {
			Get();
			var charConVal = char.Parse(t.val); 
			node = new FilterExpressionLiteralNode(typeof(char), charConVal, mu); 
			break;
		}
		case 5: {
			Get();
			var v = MakeString(t.val); 
			node = new FilterExpressionLiteralNode(typeof(string), v, mu); 
			break;
		}
		case 25: {
			Get();
			node = new FilterExpressionLiteralNode(typeof(bool), true, mu); 
			break;
		}
		case 26: {
			Get();
			node = new FilterExpressionLiteralNode(typeof(bool), false, mu); 
			break;
		}
		case 27: {
			Get();
			node = new FilterExpressionLiteralNode(null, null, mu); 
			break;
		}
		default: SynErr(50); break;
		}
	}

	void FieldReferenceOrMethodCall(out FilterExpressionNode node) {
		string qname;
		FilterExpressionNode n;
		List<FilterExpressionNode> args = null; 
		QualifiedName(out qname);
		if (la.kind == 21) {
			args = new List<FilterExpressionNode>(); 
			Get();
			if (StartOf(4)) {
				Expression(out n);
				args.Add(n); 
				while (la.kind == 23) {
					Get();
					Expression(out n);
					args.Add(n); 
				}
			}
			Expect(22);
		}
		if(args != null) node = new FilterExpressionMethodCallNode(qname, args);
		else node = new FilterExpressionFieldReferenceNode(qname); 
	}

	void Cast(out FilterExpressionNode node) {
		Type type; FilterExpressionNode n; 
		Type(out type);
		Expect(22);
		ExpressionTerminal(out n);
		node = new FilterExpressionCastNode(type, n); 
	}

	void QualifiedName(out string parts) {
		var p = new StringBuilder(); 
		Expect(1);
		p.Append(t.val); 
		while (la.kind == 24) {
			Get();
			Expect(1);
			p.Append("."); p.Append(t.val); 
		}
		parts = p.ToString(); 
	}

	void Type(out Type type) {
		Type t = null; type = null; 
		if (StartOf(5)) {
			switch (la.kind) {
			case 28: {
				Get();
				t = typeof(bool); 
				break;
			}
			case 29: {
				Get();
				t = typeof(byte); 
				break;
			}
			case 30: {
				Get();
				t = typeof(char); 
				break;
			}
			case 31: {
				Get();
				t = typeof(decimal); 
				break;
			}
			case 32: {
				Get();
				t = typeof(double); 
				break;
			}
			case 33: {
				Get();
				t = typeof(float); 
				break;
			}
			case 34: {
				Get();
				t = typeof(int); 
				break;
			}
			case 35: {
				Get();
				t = typeof(long); 
				break;
			}
			case 36: {
				Get();
				t = typeof(sbyte); 
				break;
			}
			case 37: {
				Get();
				t = typeof(short); 
				break;
			}
			case 38: {
				Get();
				t = typeof(uint); 
				break;
			}
			case 39: {
				Get();
				t = typeof(ulong); 
				break;
			}
			case 40: {
				Get();
				t = typeof(ushort); 
				break;
			}
			case 41: {
				Get();
				t = typeof(DateTime); 
				break;
			}
			case 42: {
				Get();
				t = typeof(DateTimeOffset); 
				break;
			}
			case 43: {
				Get();
				t = typeof(TimeSpan); 
				break;
			}
			}
			if (la.kind == 44) {
				Get();
				t = MakeNullable(t); 
			}
			type = t; 
		} else if (la.kind == 45) {
			Get();
			type = typeof(string); 
		} else if (la.kind == 46) {
			Get();
			type = typeof(object); 
		} else SynErr(51);
	}

	void MeasurementUnit(out string mu) {
		Expect(1);
		mu = t.val; 
	}



	public void Parse() 
	{
		la = new Token();
		la.val = "";		
		Get();
		C();
		Expect(0);

	}
	
	private static readonly bool[,] set = 
	{
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_T,_T,_x, _x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x}

	};
}

public class Errors 
{
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "intCon expected"; break;
			case 3: s = "realCon expected"; break;
			case 4: s = "charCon expected"; break;
			case 5: s = "stringCon expected"; break;
			case 6: s = "\"Or\" expected"; break;
			case 7: s = "\"And\" expected"; break;
			case 8: s = "\"Xor\" expected"; break;
			case 9: s = "\"+\" expected"; break;
			case 10: s = "\"-\" expected"; break;
			case 11: s = "\"*\" expected"; break;
			case 12: s = "\"/\" expected"; break;
			case 13: s = "\"%\" expected"; break;
			case 14: s = "\"=\" expected"; break;
			case 15: s = "\"<>\" expected"; break;
			case 16: s = "\"<\" expected"; break;
			case 17: s = "\">\" expected"; break;
			case 18: s = "\"<=\" expected"; break;
			case 19: s = "\">=\" expected"; break;
			case 20: s = "\"Not\" expected"; break;
			case 21: s = "\"(\" expected"; break;
			case 22: s = "\")\" expected"; break;
			case 23: s = "\",\" expected"; break;
			case 24: s = "\".\" expected"; break;
			case 25: s = "\"true\" expected"; break;
			case 26: s = "\"false\" expected"; break;
			case 27: s = "\"null\" expected"; break;
			case 28: s = "\"bool\" expected"; break;
			case 29: s = "\"byte\" expected"; break;
			case 30: s = "\"char\" expected"; break;
			case 31: s = "\"decimal\" expected"; break;
			case 32: s = "\"double\" expected"; break;
			case 33: s = "\"float\" expected"; break;
			case 34: s = "\"int\" expected"; break;
			case 35: s = "\"long\" expected"; break;
			case 36: s = "\"sbyte\" expected"; break;
			case 37: s = "\"short\" expected"; break;
			case 38: s = "\"uint\" expected"; break;
			case 39: s = "\"ulong\" expected"; break;
			case 40: s = "\"ushort\" expected"; break;
			case 41: s = "\"DateTime\" expected"; break;
			case 42: s = "\"DateTimeOffset\" expected"; break;
			case 43: s = "\"TimeSpan\" expected"; break;
			case 44: s = "\"?\" expected"; break;
			case 45: s = "\"string\" expected"; break;
			case 46: s = "\"object\" expected"; break;
			case 47: s = "??? expected"; break;
			case 48: s = "invalid ExpressionTerminal"; break;
			case 49: s = "invalid ExpressionTerminal"; break;
			case 50: s = "invalid Literal"; break;
			case 51: s = "invalid Type"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
}

public class FatalError: Exception 
{
	public FatalError(string m): base(m) {}
}
}