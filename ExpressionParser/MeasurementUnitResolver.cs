using DXAppProto2.FilterExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXAppProto2
{
    public class MeasurementUnitResolver
    {
        public MeasurementUnitResolver()
        {
        }

        public IMeasurementUnit GetMeasurementUnit(IMeasurementUnitRepository repo)
        {

        }

        struct FilterExpressionVisitor : IFilterExpressionVisitor
        {
            public void Visit(FilterExpressionCastNode node)
            {
                throw new NotImplementedException();
            }

            public void Visit(FilterExpressionLiteralNode node)
            {
                throw new NotImplementedException();
            }

            public void Visit(FilterExpressionMethodCallNode node)
            {
                throw new NotImplementedException();
            }

            public void Visit(FilterExpressionFieldReferenceNode node)
            {
                throw new NotImplementedException();
            }

            public void Visit(FilterExpressionUnaryNode node)
            {
                throw new NotImplementedException();
            }

            public void Visit(FilterExpressionBinaryNode node)
            {
                throw new NotImplementedException();
            }
        }
    }
}
