using Autofac.Core;
using Xunit;

namespace Autofac.Extras.Moq.Test
{
    public class DecoratorFixture
    {
        public interface IService
        {
        }

        public class Service : IService
        {
        }

        public class CachingService : IService
        {
            public CachingService(IService service)
            {
            }
        }

        public class ThingUsingService
        {
            public ThingUsingService(IService service)
            {
                Service = service;
            }

            public IService Service { get; }
        }


        [Fact]
        public void InjectsCachingServiceWhenUsingDecorator()
        {
            var autoMock = AutoMock.GetLoose(builder =>
            {
                builder.RegisterType<Service>().Named<IService>("uncached");
                builder.RegisterDecorator<IService>((ctx, inner) => new CachingService(inner), "uncached");
            });

            var sut = autoMock.Create<ThingUsingService>();

            Assert.IsType<CachingService>(sut.Service);
        }

        [Fact]
        public void InjectsCachingServiceWhenUsingRegisterTypeWorkaround()
        {
            var autoMock = AutoMock.GetLoose(builder =>
            {
                builder.RegisterType<Service>().Named<IService>("uncached");
                builder.RegisterType<CachingService>()
                    .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(IService),
                        (pi, ctx) => ctx.ResolveNamed<IService>("uncached")))
                    .As<IService>();
            });

            var sut = autoMock.Create<ThingUsingService>();

            Assert.IsType<CachingService>(sut.Service);
        }
    }
}
