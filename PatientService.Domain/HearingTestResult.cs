using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Domain
{
    /// <summary>
    /// VALUE OBJECT: Represents a hearing test result
    /// - No identity (compared by value)
    /// - Immutable (doesn't change after creation)
    /// - Represents a business concept (hearing measurement)
    /// </summary>
    public class HearingTestResult
    {
        // ===== IMMUTABLE PROPERTIES =====
        // These don't have setters - can't be changed after creation

        /// <summary>When the test was conducted</summary>
        public DateTime TestDate { get; set; }

        /// <summary>Left ear hearing level in decibels (dB)</summary>
        public int LeftEarDb { get; set; }

        /// <summary>Right ear hearing level in decibels (dB)</summary>
        public int RightEarDb { get; set; }


        // ===== BUSINESS PROPERTIES =====
        // Calculated properties that represent business rules

        /// <summary>
        /// Determines if hearing is normal
        /// BUSINESS RULE: Normal hearing is > 20dB in both ears
        /// </summary>
        public bool IsNormal => LeftEarDb > 20 && RightEarDb > 20;

        /// <summary>
        /// Gets severity level based on hearing loss
        /// BUSINESS RULE: Categorization for clinical use
        /// </summary>
        public string SeverityLevel
        {
            get
            {
                // Hearing loss categories (WHO standard)
                var worstEar = Math.Min(LeftEarDb, RightEarDb);

                return worstEar switch
                {
                    >= 21 => "Normal",
                    >= 16 and < 21 => "Slight",
                    >= 11 and < 16 => "Mild",
                    >= 6 and < 11 => "Moderate",
                    < 6 => "Severe"
                };
            }
        }

        /// <summary>
        /// Gets recommended action based on hearing level
        /// </summary>
        public string RecommendedAction
        {
            get
            {
                return SeverityLevel switch
                {
                    "Normal" => "No intervention needed",
                    "Slight" => "Monitor for changes",
                    "Mild" => "Consider hearing aids",
                    "Moderate" => "Recommend hearing aid fitting",
                    "Severe" => "Immediate fitting recommended",
                    _ => "Unknown"
                };
            }
        }


        // ===== VALUE OBJECT EQUALITY =====
        // Two HearingTestResults with same values ARE equal
        // (unlike entities which are equal only if IDs match)

        /// <summary>
        /// Compares two HearingTestResult objects by value
        /// Not by identity - if numbers are same, they're equal
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is not HearingTestResult other)
                return false;

            // VALUE COMPARISON: Same values = equal
            return LeftEarDb == other.LeftEarDb
                && RightEarDb == other.RightEarDb
                && TestDate.Date == other.TestDate.Date;  // Compare dates only
        }

        /// <summary>
        /// Hash code based on values for use in collections
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(LeftEarDb, RightEarDb, TestDate.Date);
        }

        /// <summary>
        /// Friendly string representation
        /// </summary>
        public override string ToString()
        {
            return $"HearingTest[Left:{LeftEarDb}dB Right:{RightEarDb}dB Status:{SeverityLevel}]";
        }
    }
}
