using BenefactAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace BenefactAPITests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var values = new[] { 1, 2, 3 };
            var combined = values.Select<int, Expression<Func<int, bool>>>(value => i => i > value).BinaryCombinator(Expression.Or);
        }
    }
}
