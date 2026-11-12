using System;
using System.IO;
using PetFramework;

namespace Judge
{
    internal static class Product
    {
        internal const string Repository="BurtFablehoff/TheJudge";
        internal const string AssetName="TheJudge.exe";
        internal const string StartupName="TheJudge";
        internal static ReleaseUpdater CreateUpdater()
        {
            return new ReleaseUpdater(new UpdateSettings{Repository=Repository,AssetName=AssetName,ProductName="The Judge Companion",DisplayName="The Judge",AppId="TheJudge",Cache=Path.Combine(Preferences.Folder,"Updates"),WindowTitle="The Judge"});
        }
    }
}
