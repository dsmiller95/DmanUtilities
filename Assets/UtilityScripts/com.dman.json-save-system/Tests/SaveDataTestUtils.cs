using Dman.Utilities;
using NUnit.Framework;

namespace SaveSystem.Test
{
    public class SaveDataTestUtils
    {
        public static string GetSerializedToAndAssertRoundTrip(params (string key, object data)[] datas)
        {
            var contextName = "test";

            string serializedString = SerializeToString(contextName, assertInternalRoundTrip: true, datas);
            
            AssertExternalRoundTrip(datas, contextName, serializedString);

            return serializedString;
        }

        private static void AssertExternalRoundTrip(
            (string key, object data)[] datas,
            string contextName,
            string serializedString)
        {
            using var stringStore = StringStorePersistSaveData.WithFiles((contextName, serializedString));
            var saveDataContextProvider = SaveDataContextProvider.CreateAndPersistTo(stringStore);
            saveDataContextProvider.LoadContext(contextName);
            var saveDataContext = saveDataContextProvider.GetContext(contextName);
            foreach (var (key, data) in datas)
            {
                Assert.IsTrue(saveDataContext.TryLoad(key, out var actualData, data.GetType()));
                Assert.AreEqual(data, actualData);
            }
        }

        public static bool TryLoad<T>(string serializedString, string key, out T data)
        {
            using var stringStore = StringStorePersistSaveData.WithFiles(("tmp", serializedString));
            var saveDataContextProvider = SaveDataContextProvider.CreateAndPersistTo(stringStore);
            saveDataContextProvider.LoadContext("tmp");
            var saveDataContext = saveDataContextProvider.GetContext("tmp");
            return saveDataContext.TryLoad(key, out data);
        }

        public static string SerializeToString(
            string contextName,
            bool assertInternalRoundTrip = true, 
            params (string key, object data)[] datas)
        {
            using var stringStore = new StringStorePersistSaveData();
            var saveDataContextProvider = SaveDataContextProvider.CreateAndPersistTo(stringStore);
            var saveDataContext = saveDataContextProvider.GetContext(contextName);
            foreach (var (key, data) in datas)
            {
                saveDataContext.Save(key, data);
            }
            saveDataContextProvider.PersistContext(contextName);

            if (assertInternalRoundTrip)
            {
                // assert round-trip without re-load
                foreach (var (key, data) in datas)
                {
                    Assert.IsTrue(saveDataContext.TryLoad(key, out var actualData, data.GetType()));
                    Assert.AreEqual(data, actualData);
                }
            }
                
            return stringStore.ReadFrom(contextName)!.ReadToEnd();
        }

        public static void AssertMultilineStringEqual(string expected, string actual)
        {
            expected = expected.Trim();
            actual = actual.Trim();
            if (expected == actual) return;
            Assert.Fail(StringDiffUtils.StringEqualErrorMessage(expected, actual));
        }
    }
}