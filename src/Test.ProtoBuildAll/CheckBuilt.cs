using System;
using Google.Protobuf;
using Xunit;

namespace Test.Common
{
    public class CheckBuilt
    {
        [Fact]
        public void BuiltMessages()
        {
            // first package
            Assert.NotNull(new Package1.Namespace1.Thing1());
            Assert.NotNull(new Package1.Namespace1.Thing2());
            Assert.NotNull(new Package1.Namespace2.ListOfThings());
            Assert.NotNull(new Package1.Namespace2.Thing2());

            // second package
            Assert.NotNull(new Package2.Namespace2.Payload());
        }

        [Fact]
        public void UseMessages()
        {
            var payload = new Package2.Namespace2.Payload()
            {
                Name = "payload",
                List = new Package1.Namespace2.ListOfThings()
            };

            payload.List.Things1.Add(new Package1.Namespace1.Thing1 { Name = "thing1" });
            payload.List.Things2.Add(new Package1.Namespace1.Thing2 { Name = "thing2 " });

            // serialize
            var bytes = payload.ToByteArray();
            Assert.NotNull(bytes);

            // deserialize
            var deserialized = Package2.Namespace2.Payload.Parser.ParseFrom(bytes);
            deserialized.MergeFrom(bytes);
        }

        [Fact]
        public void UseEnum()
        {
            var msg1 = new Package1.Namespace1.Thing1()
            {
                Name = "thing1",
                Kind = Package1.Namespace1.TestEnum.First
            };
            Assert.NotNull(Package1.Namespace1.Thing1.Parser.ParseFrom(msg1.ToByteArray()));

            var payload = new Package2.Namespace2.Payload()
            {
                Kind = Package1.Namespace1.TestEnum.Second
            };
            Assert.NotNull(Package2.Namespace2.Payload.Parser.ParseFrom(payload.ToByteArray()));
        }
    }
}
