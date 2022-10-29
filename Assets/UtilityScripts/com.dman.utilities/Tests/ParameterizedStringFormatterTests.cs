using NUnit.Framework;

namespace Dman.Utilities
{
    public class ParameterizedStringFormatterTests
    {
        [Test]
        public void FormatsSimpleReplacement()
        {
            var targetFormatter = new ParameterizedStringFormatter("info: {data}");
            var parameters = new ParameterizedStringFormatter.FormatParameters();
            parameters.SetParameter("data", "important info");

            Assert.AreEqual("info: important info", targetFormatter.Format(parameters));
        }
        [Test]
        public void FormatsDoubleReplacement()
        {
            var targetFormatter = new ParameterizedStringFormatter("info: {data}, {explanation}");
            var parameters = new ParameterizedStringFormatter.FormatParameters();
            parameters.SetParameter("explanation", "<- that is important");
            parameters.SetParameter("data", "important info");

            Assert.AreEqual("info: important info, <- that is important", targetFormatter.Format(parameters));
        }
        [Test]
        public void FormatsOnlyAvailableReplacements()
        {
            var targetFormatter = new ParameterizedStringFormatter("info: {data}, {explanation}");
            var parameters = new ParameterizedStringFormatter.FormatParameters();
            parameters.SetParameter("data", "important info");

            Assert.AreEqual("info: important info, {explanation}", targetFormatter.Format(parameters));
        }
    }
}
