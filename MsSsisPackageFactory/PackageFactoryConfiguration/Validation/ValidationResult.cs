using System.Text;

namespace MsSsisPackageFactory.PackageFactoryConfiguration.Validation
{
    internal class ValidationResult
    {
        internal bool IsValid { get; set; }
        internal List<string> Errors { get; set; } = new();
        internal List<string> Warnings { get; set; } = new();

        internal void AddError(string message) => Errors.Add(message);
        internal void AddWarning(string message) => Warnings.Add(message);

        internal string GetSummary()
        {
            var sb = new StringBuilder();

            if (IsValid)
            {
                sb.AppendLine("✓ Validierung erfolgreich");
            }
            else
            {
                sb.AppendLine("✗ Validierung fehlgeschlagen");
            }

            if (Errors.Any())
            {
                sb.AppendLine("Fehler:");
                foreach (var error in Errors)
                {
                    sb.AppendLine($"  - {error}");
                }
            }

            if (Warnings.Any())
            {
                sb.AppendLine("Warnungen:");
                foreach (var warning in Warnings)
                {
                    sb.AppendLine($"  - {warning}");
                }
            }

            return sb.ToString();
        }
    }
}
