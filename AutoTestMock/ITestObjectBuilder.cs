namespace AutoTestMock
{
    public interface ITestObjectBuilder
    {
        T CreateSUT<T>();

        T Dependancy<T>();
    }
}