using NUnit.Framework;

namespace TouchScript.Tests
{
    public class RuntimeTestExample
    {
        [Test]
        public void TestInt()
        {
            int a = 5;
            Assert.AreEqual(5, a);
        }
    }
}