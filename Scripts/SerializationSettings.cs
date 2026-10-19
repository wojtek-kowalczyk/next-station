using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;

namespace NS
{
    public static class SerializationSettings
    {
        public static void Set()
        {
            MessagePackSettings.Set();
        }
    }

    internal static class MessagePackSettings
    {
        public static void Set()
        {
            // this has a drawback that if a private field is of unity type it will not get serialized.
            // such cases can be worked around with serialization proxies, but it's annoying nonetheless.
            var resolver = CompositeResolver.Create(
                UnityResolver.Instance,
                StandardResolverAllowPrivate.Instance
            );

            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
                .WithResolver(resolver)
                .WithCompression(MessagePackCompression.Lz4Block);
        }
    }
}
