using NUnit.Framework;
using Rhino.Mocks;

namespace AutoTestMock.Testing
{
    [TestFixture]
    public class AutoTestMockTestFixture
    {
        [Test]
        public void GetSUTFromContainer()
        {
            var builder = new SystemUnderTestBuilder().FromAssemblyContaining<SUT>();
            var sut = builder.CreateSUT<SUT>();
            Assert.That(sut, Is.Not.Null);
        }

        [Test]
        public void GetSUTWithDependancyViaProperties()
        {
            var builder = new SystemUnderTestBuilder().FromAssemblyContaining<SUT>();
            var sut = builder.CreateSUT<SUTWithDependanciesViaProperties>();
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Dependancy, Is.Not.Null);
        }

        [Test]
        public void GetSUTWithDependancyViaConstructor()
        {
            var builder = new SystemUnderTestBuilder().FromAssemblyContaining<SUT>();
            var sut = builder.CreateSUT<SUTWithDependanciesViaConstructor>();
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Dependancy, Is.Not.Null);
        }

        [Test]
        public void GetSUTToInvokeActionOnDependancy()
        {
            var builder = new SystemUnderTestBuilder().FromAssemblyContaining<SUT>();
            var sut = builder.CreateSUT<SUTWithDependanciesViaConstructor>();
            builder.Dependancy<IDependancy>().Expect(d => d.ReturnValue()).Return("hello world");
            Assert.That(sut.GetObjectOfDependancy(), Is.EqualTo("hello world"));

            builder.Dependancy<IDependancy>().VerifyAllExpectations();
        }

        public interface IDependancy
        {
            string ReturnValue();
        }

        public interface ISecondDependancy
        {
        }

        public class SUT
        {
        }

        public class SUTWithDependanciesViaProperties
        {
            public IDependancy Dependancy { get; set; }
        }

        public class SUTWithDependanciesViaConstructor
        {
            private readonly ISecondDependancy secondDependancy;

            public SUTWithDependanciesViaConstructor(IDependancy dependancy, ISecondDependancy secondDependancy)
            {
                this.secondDependancy = secondDependancy;
                this.Dependancy = dependancy;
            }

            public IDependancy Dependancy { get; private set; }

            public string GetObjectOfDependancy()
            {
                return Dependancy.ReturnValue();
            }
        }
    }
}
