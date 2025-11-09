using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Domain
{
    /// <summary>
    /// ENTITY: Patient is a core business concept
    /// - Has unique identity (ID)
    /// - Has lifecycle (created, updated, deleted)
    /// - Encapsulates business rules for patients
    /// </summary>
    public class Patient
    {
        // ===== PROPERTIES =====
        // These represent the patient's data

        /// <summary>Unique identifier for this patient</summary>
        public string Id { get; set; }

        /// <summary>Patient's full name</summary>
        public string Name { get; set; }

        /// <summary>Patient's email for communication</summary>
        public string Email { get; set; }

        /// <summary>Most recent hearing test result</summary>
        public HearingTestResult LatestTest { get; set; }

        /// <summary>IDs of hearing aids assigned to this patient</summary>
        public List<string> DeviceIds { get; set; } = new();

        /// <summary>When this patient was created</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>When this patient was last modified</summary>
        public DateTime ModifiedDate { get; set; }


        // ===== FACTORY METHOD =====
        // Creates a new patient with validation
        // "Factory Pattern" - encapsulates creation logic

        /// <summary>
        /// Creates a new patient with validation
        /// </summary>
        /// <param name="name">Patient's name (required)</param>
        /// <param name="email">Patient's email (required)</param>
        /// <returns>New Patient instance</returns>
        /// <exception cref="ArgumentException">If name or email is empty</exception>
        public static Patient Create(string name, string email)
        {
            // BUSINESS RULE: Name is required
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Patient name is required", nameof(name));

            // BUSINESS RULE: Email is required
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Patient email is required", nameof(email));

            // Create new instance with business rules applied
            return new Patient
            {
                Id = Guid.NewGuid().ToString(),  // Generate unique ID
                Name = name.Trim(),
                Email = email.Trim().ToLower(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };
        }


        // ===== BUSINESS METHODS =====
        // These encapsulate business logic specific to Patient

        /// <summary>
        /// Records a hearing test result for this patient
        /// BUSINESS RULE: Updates the latest test and modified date
        /// </summary>
        public void RecordHearingTest(int leftEarDb, int rightEarDb)
        {
            // VALIDATION: Hearing measurement must be non-negative
            if (leftEarDb < 0 || rightEarDb < 0)
                throw new ArgumentException("Hearing measurements cannot be negative");

            // Update the latest test result
            LatestTest = new HearingTestResult
            {
                TestDate = DateTime.UtcNow,
                LeftEarDb = leftEarDb,
                RightEarDb = rightEarDb
            };

            // BUSINESS RULE: Update modified date when patient data changes
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Assigns a hearing device to this patient
        /// BUSINESS RULE: Don't add duplicate devices
        /// </summary>
        public void AssignDevice(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID required", nameof(deviceId));

            // Don't add the same device twice
            if (DeviceIds.Contains(deviceId))
                throw new InvalidOperationException($"Device {deviceId} already assigned");

            DeviceIds.Add(deviceId);
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates patient contact information
        /// BUSINESS RULE: Email must be provided to update
        /// </summary>
        public void UpdateContactInfo(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name required", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email required", nameof(email));

            Name = name.Trim();
            Email = email.Trim().ToLower();
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if this patient has normal hearing based on latest test
        /// </summary>
        public bool HasNormalHearing()
        {
            return LatestTest?.IsNormal ?? false;
        }
    }
}
