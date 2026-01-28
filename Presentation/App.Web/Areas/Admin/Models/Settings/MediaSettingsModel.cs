using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Settings
{
    /// <summary>
    /// Represents a media settings model
    /// </summary>
    public partial record MediaSettingsModel : BaseNopModel, ISettingsModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase")]
        public bool PicturesStoredIntoDatabase { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.AvatarPictureSize")]
        public int AvatarPictureSize { get; set; }
        public bool AvatarPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.MaximumImageSize")]
        public int MaximumImageSize { get; set; }
        public bool MaximumImageSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.MultipleThumbDirectories")]
        public bool MultipleThumbDirectories { get; set; }
        public bool MultipleThumbDirectories_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.DefaultImageQuality")]
        public int DefaultImageQuality { get; set; }
        public bool DefaultImageQuality_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ImportProductImagesUsingHash")]
        public bool ImportProductImagesUsingHash { get; set; }
        public bool ImportProductImagesUsingHash_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.DefaultPictureZoomEnabled")]
        public bool DefaultPictureZoomEnabled { get; set; }
        public bool DefaultPictureZoomEnabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.AllowSVGUploads")]
        public bool AllowSVGUploads { get; set; }
        public bool AllowSVGUploads_OverrideForStore { get; set; }

        #endregion
    }
}