using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Domain
{
    /// <summary>
    /// REPOSITORY INTERFACE: Contract for accessing Patient data
    /// 
    /// WHY: Allows abstraction of data access
    /// - Real implementation: connects to database
    /// - Test implementation: uses in-memory list
    /// - Future implementation: could use API, cache, etc.
    /// 
    /// BENEFIT: Business logic doesn't care HOW data is stored
    /// </summary>
    public interface IPatientRepository
    {
        // ===== READ OPERATIONS =====

        /// <summary>
        /// Gets a patient by their unique identifier
        /// </summary>
        /// <param name="id">Patient ID to retrieve</param>
        /// <returns>Patient if found, null otherwise</returns>
        Task<Patient> GetByIdAsync(string id);

        /// <summary>
        /// Gets all patients
        /// </summary>
        /// <returns>Collection of all patients</returns>
        Task<List<Patient>> GetAllAsync();

        /// <summary>
        /// Finds patients by name (partial match, case-insensitive)
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>Patients matching the name</returns>
        Task<List<Patient>> GetByNameAsync(string name);


        // ===== WRITE OPERATIONS =====

        /// <summary>
        /// Adds a new patient to the repository
        /// </summary>
        /// <param name="patient">Patient to add (must be valid)</param>
        /// <exception cref="ArgumentNullException">If patient is null</exception>
        Task AddAsync(Patient patient);

        /// <summary>
        /// Updates an existing patient
        /// </summary>
        /// <param name="patient">Patient with updated data</param>
        /// <exception cref="KeyNotFoundException">If patient doesn't exist</exception>
        Task UpdateAsync(Patient patient);

        /// <summary>
        /// Deletes a patient by ID
        /// </summary>
        /// <param name="id">ID of patient to delete</param>
        Task DeleteAsync(string id);
    }
}
