using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsSsisPackageFactory
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public void AddError(string message) => Errors.Add(message);
        public void AddWarning(string message) => Warnings.Add(message);

        public string GetSummary()
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
