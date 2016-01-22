# ExpressionParser

[![Build status](https://ci.appveyor.com/api/projects/status/sba8smc1b2bl6wax?svg=true)](https://ci.appveyor.com/project/jairov4/expressionparser)

This measurement units aware expression parser is designed to allow express simple math expressions that include measurement units as part of itself.

  p.e.   `25cm + 36m - sin(t)*6mm`

The master plan is:

- To use COCO/R to generate the expresion compiler for C# [http://www.ssw.uni-linz.ac.at/Coco/]
- Generate an Abstract Syntax Tree for compiled expressions
- Use the Dimensional Analyzer class to perform algebraic manipulation for measurement units handled.
