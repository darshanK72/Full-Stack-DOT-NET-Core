/*
 * PROBLEM: Clinic Appointment Manager
 *
 * A small medical clinic tracks its doctors and appointments through a
 * back-end manager class.  The reception desk registers doctors, books
 * appointments, and runs statistical reports to support staffing decisions.
 *
 * This is a manager-class style problem — the focus is on the analytics methods,
 * not on a user-facing menu.  Main runs a scripted demo with known data and
 * expected outputs printed as verification.
 *
 * This exercise covers:
 *   ch02 — enums for appointment status and type; int for durations and counts
 *   ch04 — comparisons for sorting; integer cast to double before averaging
 *   ch06 — loops over collections; sorting a list with a custom comparison
 *   ch07 — bool returns from Add methods; all analytics return typed results
 *   ch09 — Dictionary for per-type and per-doctor aggregations; List for rankings
 */

using System;
using System.Collections.Generic;

namespace HealthcareScheduling
{
    /*
     * Tracks the lifecycle of an appointment from creation through completion.
     * Only COMPLETED appointments are counted in the analytics methods.
     */
    enum AppointmentStatus
    {
        SCHEDULED, COMPLETED, CANCELLED, NO_SHOW
    }

    /*
     * Classifies the clinical nature of an appointment.
     * All three types must appear in the result of GetAverageDurationByType,
     * even if a type has no completed appointments.
     */
    enum AppointmentType
    {
        CONSULTATION, FOLLOWUP, EMERGENCY
    }

    /*
     * Represents a clinician registered in the system.
     */
    class Doctor
    {
        public int    DoctorId { get; set; }
        public string Name     { get; set; }
    }

    /*
     * Represents one booked slot.
     * DurationMinutes must be greater than zero — enforced by AddAppointment.
     */
    class Appointment
    {
        public int               AppointmentId   { get; set; }
        public int               DoctorId        { get; set; }
        public int               PatientId       { get; set; }
        public AppointmentType   Type            { get; set; }
        public AppointmentStatus Status          { get; set; }
        public int               DurationMinutes { get; set; }
    }

    /*
     * Manages the clinic's doctor roster and appointment log.
     * Provides analytics methods used by the reporting team.
     */
    class ClinicManager
    {
        private List<Doctor>      _doctors      = new List<Doctor>();
        private List<Appointment> _appointments = new List<Appointment>();

        /*
         * Registers a doctor.  Silently ignores the call and returns false when a
         * doctor with the same DoctorId is already registered.
         * Returns true when the doctor was successfully added.
         */
        public bool AddDoctor(Doctor doctor)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Logs a new appointment.
         *
         * Returns false — and does not add the appointment — when the referenced
         * DoctorId has not been registered, or when DurationMinutes is not positive.
         * Returns true when the appointment is successfully stored.
         */
        public bool AddAppointment(Appointment appt)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Returns the total number of appointments whose status is COMPLETED.
         */
        public int GetCompletedCount()
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Returns a dictionary mapping every AppointmentType to the average
         * DurationMinutes across COMPLETED appointments of that type.
         *
         * All three enum values must appear in the result.
         * Types that have no completed appointments map to 0.0.
         */
        public Dictionary<AppointmentType, double> GetAverageDurationByType()
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Returns a ranked list of doctor Ids based on total minutes spent in
         * COMPLETED appointments, highest first.
         *
         * When two doctors have equal total minutes the one with the lower DoctorId
         * ranks higher.  At most topK entries are returned.
         * Returns an empty list immediately when topK is zero or negative.
         */
        public List<int> GetDoctorWorkloadRanking(int topK)
        {
            // TODO: implement
            throw new NotImplementedException();
        }
    }

    /*
     * Entry point — scripted demo.
     *
     * Seeds a fixed set of doctors and appointments, then prints the result of
     * each analytics method alongside the expected value so you can verify correctness.
     */
    class Program
    {
        static void Main(string[] args)
        {
            ClinicManager manager = new ClinicManager();

            /*
             * Seed doctors: three doctors with Ids 1, 2, 3.
             */

            /*
             * Seed appointments — include a mix of COMPLETED, SCHEDULED, and CANCELLED
             * so that the analytics methods are forced to filter correctly:
             *
             *   Appt 1: Doctor 1, CONSULTATION, COMPLETED, 30 min
             *   Appt 2: Doctor 1, EMERGENCY,    COMPLETED, 40 min
             *   Appt 3: Doctor 2, CONSULTATION, COMPLETED, 20 min
             *   Appt 4: Doctor 3, FOLLOWUP,     SCHEDULED, 15 min  <- not COMPLETED
             */

            /*
             * Expected output after all methods are implemented:
             *
             *   Completed count: 3
             *
             *   Average duration by type:
             *     CONSULTATION : 25.0 min   (Doctor 1 had 30, Doctor 2 had 20 -> avg 25)
             *     FOLLOWUP     :  0.0 min   (no completed FOLLOWUP appointments)
             *     EMERGENCY    : 40.0 min
             *
             *   Workload ranking (top 2):
             *     1, 2          (Doctor 1 = 70 min total; Doctor 2 = 20 min total)
             */

            // TODO: call AddDoctor, AddAppointment, then print each analytics method result
            throw new NotImplementedException();
        }
    }
}
