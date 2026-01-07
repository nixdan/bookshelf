using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Hardcover
{
    public class HardcoverImportSettingsValidator : AbstractValidator<HardcoverImportSettings>
    {
        public HardcoverImportSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c)
                .Must(c => (c.ListIds != null && c.ListIds.Any()) || (c.BookStatusIds != null && c.BookStatusIds.Any()))
                .WithMessage("At least one List or Book Status must be selected");
        }
    }

    public class HardcoverImportSettings : IImportListSettings
    {
        private static readonly HardcoverImportSettingsValidator Validator = new ();

        public HardcoverImportSettings()
        {
            BaseUrl = "https://api.hardcover.app";
            ListIds = Array.Empty<string>();
            BookStatusIds = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "Base URL", HelpText = "Hardcover API base URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Hardcover personal API key (from Settings > API)")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, SelectOptionsProviderAction = "getLists", Label = "Lists", HelpText = "Choose lists from your Hardcover account to sync (optional if Book Statuses selected)")]
        public IEnumerable<string> ListIds { get; set; }

        [FieldDefinition(3, Type = FieldType.Select, SelectOptions = typeof(HardcoverBookStatus), Label = "Book Statuses", HelpText = "Select which book statuses to import (optional if Lists selected)")]
        public IEnumerable<int> BookStatusIds { get; set; }

        public string ListId => ListIds?.FirstOrDefault();

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum HardcoverBookStatus
    {
        [FieldOption(Label = "Want to Read")]
        WantToRead = 1,

        [FieldOption(Label = "Currently Reading")]
        CurrentlyReading = 2,

        [FieldOption(Label = "Read")]
        Read = 3,

        [FieldOption(Label = "Paused")]
        Paused = 4,

        [FieldOption(Label = "Did Not Finish")]
        DidNotFinish = 5,

        [FieldOption(Label = "Ignored")]
        Ignored = 6
    }
}
