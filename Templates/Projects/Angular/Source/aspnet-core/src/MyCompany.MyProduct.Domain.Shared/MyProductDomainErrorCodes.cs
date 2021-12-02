namespace MyCompany.MyProduct
{
    public static class MyProductDomainErrorCodes
    {
        // You can add your business exception error codes here, as constants. Example:
        public static class Samples
        {
            public const string AlreadyExists = _prefix + nameof(AlreadyExists);
            public const string NotFound = _prefix + nameof(NotFound);
            private const string _prefix = $"{nameof(Samples)}:";
        }
    }
}