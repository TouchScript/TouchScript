using NUnit.Framework;

namespace TouchScript.Tests
{
    public class EditorTestExample
    {
        [Test]
        public void TestString()
        {
            string foo = "bar";
            Assert.AreEqual("bar", foo);
        }
    }
}