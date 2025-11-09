using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatientService.Domain;

namespace PatientService.Application
{
    /// <summary>
    /// APPLICATION SERVICE: Orchestrates business processes
    /// 
    /// WHY: Sits between API and Domain/Infrastructure
    /// - API doesn't know about repository, domain, or data access
    /// - Domain stays focused on business rules only
    /// - Easy to test business logic independently
    /// 
    /// This service coordinates:
    /// - Getting patients from repository
    /// - Creating new patients
    /// - Applying business rules
    /// - Returning results to API
    /// </summary>
    public class PatientApplicationService
    {
        private readonly IPatientRepository _repository;

        /// <summary>
        /// DEPENDENCY INJECTION: Repository provided from outside
        /// This allows flexibility - can inject real DB or test fake
        /// </summary>
        public PatientApplicationService(IPatientRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        // ===== CREATE OPERATIONS =====

        /// <summary>
        /// Creates a new patient
        /// BUSINESS PROCESS: Validate input → Create entity → Save → Return
        /// </summary>
        public async Task<PatientDto> CreatePatientAsync(CreatePatientRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Create domain entity with validation
            var patient = Patient.Create(request.Name, request.Email);

            // Save to repository
            await _repository.AddAsync(patient);

            // Return DTO to caller
            return PatientDto.FromDomain(patient);
        }


        // ===== READ OPERATIONS =====

        /// <summary>
        /// Gets a patient by ID
        /// Returns null if not found
        /// </summary>
        public async Task<PatientDto> GetPatientAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Patient ID required", nameof(id));

            var patient = await _repository.GetByIdAsync(id);

            if (patient == null)
                throw new KeyNotFoundException($"Patient {id} not found");

            return PatientDto.FromDomain(patient);
        }

        /// <summary>
        /// Gets all patients
        /// Returns list of DTOs
        /// </summary>
        public async Task<List<PatientDto>> GetAllPatientsAsync()
        {
            var patients = await _repository.GetAllAsync();
            return patients.Select(PatientDto.FromDomain).ToList();
        }

        /// <summary>
        /// Searches patients by name
        /// Case-insensitive, partial match
        /// </summary>
        public async Task<List<PatientDto>> SearchPatientsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Search name required", nameof(name));

            var patients = await _repository.GetByNameAsync(name);
            return patients.Select(PatientDto.FromDomain).ToList();
        }


        // ===== UPDATE OPERATIONS =====

        /// <summary>
        /// Updates patient contact information
        /// BUSINESS PROCESS: Get existing → Update → Save
        /// </summary>
        public async Task UpdatePatientAsync(string id, UpdatePatientRequest request)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Patient ID required", nameof(id));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Get existing patient
            var patient = await _repository.GetByIdAsync(id);
            if (patient == null)
                throw new KeyNotFoundException($"Patient {id} not found");

            // Update using domain method
            patient.UpdateContactInfo(request.Name ?? patient.Name, request.Email ?? patient.Email);

            // Save updated patient
            await _repository.UpdateAsync(patient);
        }

        /// <summary>
        /// Records a hearing test result for a patient
        /// BUSINESS LOGIC: Updates patient's test results
        /// </summary>
        public async Task RecordHearingTestAsync(string patientId, int leftEarDb, int rightEarDb)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("Patient ID required", nameof(patientId));

            // Get patient
            var patient = await _repository.GetByIdAsync(patientId);
            if (patient == null)
                throw new KeyNotFoundException($"Patient {patientId} not found");

            // Use domain method to record test (validates dB values)
            patient.RecordHearingTest(leftEarDb, rightEarDb);

            // Save updated patient
            await _repository.UpdateAsync(patient);
        }


        // ===== DELETE OPERATIONS =====

        /// <summary>
        /// Deletes a patient
        /// Removes all associated data
        /// </summary>
        public async Task DeletePatientAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Patient ID required", nameof(id));

            await _repository.DeleteAsync(id);
        }


        // ===== ADVANCED: PARALLEL ASYNC OPERATIONS =====

        /// <summary>
        /// ADVANCED: Gets complete patient data in parallel
        /// Shows async/await patterns with Task.WhenAll
        /// 
        /// If each operation takes 100ms:
        /// - Sequential: 100ms + 100ms + 100ms = 300ms total
        /// - Parallel (WhenAll): max(100ms, 100ms, 100ms) = 100ms total
        /// 3x FASTER!
        /// </summary>
        public async Task<PatientComprehensiveDto> GetCompletePatientDataAsync(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("Patient ID required", nameof(patientId));

            // Start all operations in parallel
            var patientTask = _repository.GetByIdAsync(patientId);
            var appointmentsTask = FetchAppointmentsAsync(patientId);
            var devicesTask = FetchDevicesAsync(patientId);

            // Wait for ALL to complete (in parallel, not sequential)
            await Task.WhenAll(patientTask, appointmentsTask, devicesTask);

            // Collect results
            var patient = patientTask.Result;
            if (patient == null)
                throw new KeyNotFoundException($"Patient {patientId} not found");

            return new PatientComprehensiveDto
            {
                Patient = PatientDto.FromDomain(patient),
                Appointments = appointmentsTask.Result,
                Devices = devicesTask.Result
            };
        }

        /// <summary>
        /// Simulates fetching appointments for a patient
        /// In real system, would call another microservice
        /// </summary>
        private async Task<List<string>> FetchAppointmentsAsync(string patientId)
        {
            // Simulate API call/database query
            await Task.Delay(50);
            return new List<string> { "Appointment 1", "Appointment 2" };
        }

        /// <summary>
        /// Simulates fetching devices for a patient
        /// In real system, would call device service
        /// </summary>
        private async Task<List<string>> FetchDevicesAsync(string patientId)
        {
            // Simulate API call/database query
            await Task.Delay(50);
            return new List<string> { "Device 1" };
        }
    }


    // ===== DATA TRANSFER OBJECTS (DTOs) =====
    // These convert domain models to API responses
    // WHY: Decouples what API returns from domain structure

    /// <summary>
    /// DTO: Patient data for API responses
    /// Converts domain Patient to simpler form
    /// </summary>
    public class PatientDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public HearingTestResultDto LatestTest { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Converts domain Patient to DTO
        /// </summary>
        public static PatientDto FromDomain(Patient patient)
        {
            return new PatientDto
            {
                Id = patient.Id,
                Name = patient.Name,
                Email = patient.Email,
                LatestTest = patient.LatestTest != null
                    ? new HearingTestResultDto
                    {
                        TestDate = patient.LatestTest.TestDate,
                        LeftEarDb = patient.LatestTest.LeftEarDb,
                        RightEarDb = patient.LatestTest.RightEarDb,
                        IsNormal = patient.LatestTest.IsNormal,
                        SeverityLevel = patient.LatestTest.SeverityLevel
                    }
                    : null,
                CreatedDate = patient.CreatedDate
            };
        }
    }

    /// <summary>
    /// DTO: Hearing test result for API responses
    /// </summary>
    public class HearingTestResultDto
    {
        public DateTime TestDate { get; set; }
        public int LeftEarDb { get; set; }
        public int RightEarDb { get; set; }
        public bool IsNormal { get; set; }
        public string SeverityLevel { get; set; }
    }

    /// <summary>
    /// DTO: Complete patient data (patient + appointments + devices)
    /// </summary>
    public class PatientComprehensiveDto
    {
        public PatientDto Patient { get; set; }
        public List<string> Appointments { get; set; }
        public List<string> Devices { get; set; }
    }


    // ===== REQUEST MODELS =====
    // These are what API receives from clients

    /// <summary>
    /// Request to create a new patient
    /// </summary>
    public class CreatePatientRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    /// <summary>
    /// Request to update patient
    /// </summary>
    public class UpdatePatientRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    /// <summary>
    /// Request to record hearing test
    /// </summary>
    public class RecordHearingTestRequest
    {
        public int LeftEarDb { get; set; }
        public int RightEarDb { get; set; }
    }
}
