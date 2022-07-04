using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Milochau.Emails.Sdk.Models
{
    /// <summary>Email attachment</summary>
    public class EmailAttachment
    {
        /// <summary>Characters forbidden in file names, filtered by the <see cref="GetNormalizedFileName"/> method</summary>
        public readonly IReadOnlyList<char> NonRenderedCharacters = new List<char> { ';', ',', '\n', '\r' };

        /// <summary>File name</summary>
        /// <remarks>This must be the exact file name, as you can find in the storage</remarks>
        public string FileName { get; set; } = null!;

        /// <summary>Public file name</summary>
        /// <remarks>If a <see cref="PublicFileName"/> is not provided, the <see cref="FileName"/> will be used</remarks>
        public string? PublicFileName { get; set; }

        /// <summary>Get a normalized file name, from <see cref="PublicFileName"/> and <see cref="FileName"/></summary>
        /// <returns>The normalized file name, or null if no normalized file name can be created</returns>
        /// <remarks>This method remove accents</remarks>
        public string? GetNormalizedFileName()
        {
            if (!string.IsNullOrWhiteSpace(PublicFileName))
            {
                var normalizedPublicFileName = RenderNormalized(PublicFileName!);

                if (!string.IsNullOrWhiteSpace(normalizedPublicFileName))
                {
                    return normalizedPublicFileName;
                }
            }
            else if (!string.IsNullOrWhiteSpace(FileName))
            {
                var normalizedFileName = RenderNormalized(FileName!);

                if (!string.IsNullOrWhiteSpace(normalizedFileName))
                {
                    return normalizedFileName;
                }
            }
            return null;
        }

        private string RenderNormalized(string input)
        {
            return new string(input
                .Normalize(System.Text.NormalizationForm.FormD) // Separate accents from letters
                .ToCharArray()
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) // Remove all non-spacing chars, including accents
                .Where(c => !NonRenderedCharacters.Contains(c))
                .ToArray());
        }
    }
}
