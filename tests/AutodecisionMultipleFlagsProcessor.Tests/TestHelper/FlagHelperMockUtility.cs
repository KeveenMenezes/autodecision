using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Services;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests.TestHelper
{
    public static class FlagHelperMockUtility
    {
        public static void AddDefaultBehaviors(Mock<IFlagHelper> mockFlagHelper)
        {
            MockFlagResponseEvent(mockFlagHelper);
        }

        public static void MockFeatureToggleInstance(Mock<IFeatureToggleClient> mockFeatureToggle, bool isEnabled)
        {
            mockFeatureToggle
               .Setup(x => x.IsEnabled(It.IsAny<string>()))
               .Returns(isEnabled);
        }

        private static void MockFlagResponseEvent(Mock<IFlagHelper> mock)
        {
            mock
                .Setup(x => x.BuildFlagResponse(It.IsAny<string>(), It.IsAny<AutodecisionCompositeData>()))
                .Returns((string s, AutodecisionCompositeData c) =>
                {
                    return new ProcessFlagResponseEvent()
                    {
                        FlagCode = s,
                        LoanNumber = c.Application.LoanNumber,
                        Version = c.Version,
                        ProcessedAt = DateTime.UtcNow,
                    };
                });

            mock
                .Setup(x => x.BuildFlagResponse(It.IsAny<string>(), It.IsAny<AutodecisionCompositeData>(), It.IsAny<FlagResultEnum>()))
                .Returns((string s, AutodecisionCompositeData c, FlagResultEnum f) =>
                {
                    return new ProcessFlagResponseEvent()
                    {
                        FlagCode = s,
                        LoanNumber = c.Application.LoanNumber,
                        Version = c.Version,
                        ProcessedAt = DateTime.UtcNow,
                        FlagResult = f
                    };
                });

            mock
                .Setup(x => x.RaiseFlag(It.IsAny<ProcessFlagResponseEvent>(), It.IsAny<string>()))
                .Callback((ProcessFlagResponseEvent e, string r) =>
                {
                    e.Message = r;
                    e.FlagResult = FlagResultEnum.PendingApproval;
                });
        }
    }
}