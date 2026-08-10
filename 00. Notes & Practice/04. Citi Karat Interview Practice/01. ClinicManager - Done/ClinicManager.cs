/*
 * <bug-task1>
 * We are building the back-end for a clinic appointment management system.
 * The system tracks doctors and appointments.
 *
 * Definitions:
 * - A "doctor" has: doctorId, name.
 * - An "appointment" has: appointmentId, doctorId, patientId,
 *   durationMinutes, status, appointmentType.
 * - AppointmentStatus is one of: SCHEDULED, COMPLETED, CANCELLED, NO_SHOW.
 * - AppointmentType is one of: CONSULTATION, FOLLOWUP, EMERGENCY.
 * - "ClinicManager" manages doctors, appointments, and provides statistics.
 *
 * To begin with, we present you with two tasks:
 * 1-1) Read through and understand the code below. Feel free to run it.
 * 1-2) The test for ClinicManager is not passing due to a bug in the code.
 *      Make the necessary changes to ClinicManager to fix the bug.
 * </bug-task1>
 */

/*
 * <task2>
 * We are extending the platform to report on appointment durations by type.
 *
 * Recall that each Appointment carries an appointmentType, one of
 * CONSULTATION, FOLLOWUP, EMERGENCY.
 *
 * Add a new method to the ClinicManager class:
 *
 * 2) getAverageAppointmentDurationByType(int doctorId) should return a
 *    Map mapping each AppointmentType to the doctor's average appointment
 *    duration in minutes for that type. Only COMPLETED appointments count;
 *    ignore appointments with any other status. Only types with at least
 *    one completed appointment appear in the map. If the doctor has no
 *    completed appointments at all, return an empty map.
 *
 * To assist you in testing this new method, we have provided the
 * testGetAverageAppointmentDurationByType test.
 * </task2>
 */

/*
 * <task3>
 * We want to generate an appointment activity summary for each doctor in the system.
 *
 * For each doctor, the summary captures how busy they are overall and
 * what kind of appointments they handle most often.
 *
 * DoctorAppointmentSummary captures:
 * - totalAppointments    : total number of appointments for that doctor
 * - totalMinutes         : sum of all durationMinutes for that doctor
 * - busiestAppointmentType : type with the most appointments for that doctor.
 *                           On a tie, pick the one that comes first alphabetically.
 *                           If the doctor has no appointments, set to null.
 *
 * 3) getDoctorAppointmentSummary() returns a Map from doctorId to
 *    DoctorAppointmentSummary for every doctor in the system, regardless of
 *    appointment status. Doctors with no appointments get 0 / 0 / null.
 *
 * To assist you in testing this new method, we have provided the
 * testGetDoctorAppointmentSummary test.
 * </task3>
 */

/*
 * <task4>
 * We want to rank doctors by their overall workload and identify the
 * patient who has the most appointments with each of them.
 *
 * 4) getDoctorWorkloadRanking(int k) returns a List of int[] entries —
 *    one per qualifying doctor — where each entry is
 *    { doctorId, totalMinutes, mostFrequentPatientId }.
 *
 *    For each doctor with at least one appointment:
 *    - totalMinutes is the sum of all durationMinutes for that doctor.
 *    - mostFrequentPatientId is the patientId with the most appointments
 *      with that doctor. On a tie, pick the smaller patientId.
 *    Only include doctors who have at least one appointment.
 *    Sort by totalMinutes descending; break ties by doctorId ascending.
 *    Return only the top k entries.
 *    If k <= 0, return an empty list.
 *
 * To assist you in testing this new method, we have provided the
 * testGetDoctorWorkloadRankingCase1 and testGetDoctorWorkloadRankingCase2 tests.
 * </task4>
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    enum AppointmentStatus
    {
        SCHEDULED, COMPLETED, CANCELLED, NO_SHOW
    }

    enum AppointmentType
    {
        CONSULTATION, FOLLOWUP, EMERGENCY
    }

    class Doctor
    {
        public int doctorId;
        public string name;

        public Doctor(int doctorId, string name)
        {
            this.doctorId = doctorId;
            this.name = name;
        }
    }

    class Appointment
    {
        public int appointmentId;
        public int doctorId;
        public int patientId;
        public int durationMinutes;
        public AppointmentStatus status;
        public AppointmentType appointmentType;

        public Appointment(int appointmentId, int doctorId, int patientId,
                    int durationMinutes, AppointmentStatus status, AppointmentType appointmentType)
        {
            this.appointmentId = appointmentId;
            this.doctorId = doctorId;
            this.patientId = patientId;
            this.durationMinutes = durationMinutes;
            this.status = status;
            this.appointmentType = appointmentType;
        }
    }

    class AppointmentStats
    {
        public int totalAppointments;
        public int completedAppointments;
        public double noShowRate;

        public AppointmentStats(int totalAppointments, int completedAppointments, double noShowRate)
        {
            this.totalAppointments = totalAppointments;
            this.completedAppointments = completedAppointments;
            this.noShowRate = noShowRate;
        }
    }

    class DoctorAppointmentSummary
    {
        public int totalAppointments;
        public int totalMinutes;
        public AppointmentType? busiestAppointmentType;

        public DoctorAppointmentSummary(int totalAppointments, int totalMinutes, AppointmentType? busiestAppointmentType)
        {
            this.totalAppointments = totalAppointments;
            this.totalMinutes = totalMinutes;
            this.busiestAppointmentType = busiestAppointmentType;
        }
    }

    class ClinicManager
    {
        private Dictionary<int, Doctor> doctors = new Dictionary<int, Doctor>();
        private List<Appointment> appointments = new List<Appointment>();

        public void AddDoctor(Doctor doctor)
        {
            doctors[doctor.doctorId] = doctor;
        }

        public void AddAppointment(Appointment appointment)
        {
            if (!doctors.ContainsKey(appointment.doctorId)) return;
            appointments.Add(appointment);
        }

        // TASK 1: This method has a bug. Find and fix it.
        public AppointmentStats GetAppointmentStatistics()
        {
            int total = appointments.Count;

            int completed = 0;
            foreach (Appointment a in appointments)
            {
                // checking whether appointment status is completed then only increment
                if (a.status == AppointmentStatus.COMPLETED)
                {
                    completed++;
                }
            }

            int noShows = 0;
            foreach (Appointment a in appointments)
            {
                if (a.status == AppointmentStatus.NO_SHOW) noShows++;
            }

            double noShowRate = total > 0 ? (double)noShows / total : 0.0;
            return new AppointmentStats(total, completed, noShowRate);
        }

        /**
         * TASK 2: getAverageAppointmentDurationByType(int doctorId) should return a
         * Map mapping each AppointmentType to the doctor's average appointment
         * duration in minutes for that type. Only COMPLETED appointments count;
         * ignore appointments with any other status. Only types with at least
         * one completed appointment appear in the map. If the doctor has no
         * completed appointments at all, return an empty map.
         */
        public Dictionary<AppointmentType, double> GetAverageAppointmentDurationByType(int doctorId)
        {
            return this.appointments
                .Where(app => app.doctorId == doctorId && app.status == AppointmentStatus.COMPLETED)
                .GroupBy(app => app.appointmentType)
                .ToDictionary(group => group.Key,group => group.Average(app => app.durationMinutes));
        }

        /**
         * TASK 3: getDoctorAppointmentSummary() returns a Map from doctorId to
         * DoctorAppointmentSummary for every doctor in the system, regardless of
         * appointment status. Doctors with no appointments get 0 / 0 / null.
         */
        public Dictionary<int, DoctorAppointmentSummary> GetDoctorAppointmentSummary()
        {
            // TODO: implement
            //return new Dictionary<int, DoctorAppointmentSummary>();
            Dictionary<int, DoctorAppointmentSummary> output = new Dictionary<int, DoctorAppointmentSummary>();
            foreach (Doctor doc in this.doctors.Values){
                List<Appointment> docAppoIntments = this.appointments.Where(app => app.doctorId == doc.doctorId).ToList();
                int totalAppointments = docAppoIntments.Count;

                if(totalAppointments == 0){
                    output[doc.doctorId] = new DoctorAppointmentSummary(0,0,null);
                    continue;
                }

                int totalDuration = docAppoIntments.Sum(app => app.durationMinutes);


                AppointmentType type = docAppoIntments
                                        .GroupBy(app => app.appointmentType)
                                        .OrderByDescending(g => g.Count())
                                        .ThenBy(g => g.Key.ToString())
                                        .First()
                                        .Key;

                output[doc.doctorId] = new DoctorAppointmentSummary(totalAppointments,totalDuration,type);
            }
            return output;
        }

        /**
         * TASK 4: getDoctorWorkloadRanking(int k) returns a List of int[] entries —
         * one per qualifying doctor — where each entry is
         * { doctorId, totalMinutes, mostFrequentPatientId }.
         */
        public List<int[]> GetDoctorWorkloadRanking(int k)
        {
            // TODO: implement
            //return new List<int[]>();

            List<int[]> output = new List<int[]>();

            foreach(Doctor doc in this.doctors.Values)
            {
                var appoits = this.appointments.Where(appo => appo.doctorId == doc.doctorId).ToList();
                if (appoits.Count == 0) continue;

                int totalMinutes = appoits.Sum(app => app.durationMinutes);

                int mostFrequentPatientId = appoits
                                    .GroupBy(app => app.patientId)
                                    .OrderByDescending(g => g.Count())
                                    .ThenBy(g => g.Key)
                                    .First()
                                    .Key;

                output.Add(new int[]
                {
                    doc.doctorId,
                    totalMinutes,
                    mostFrequentPatientId
                });
            }

            return output.OrderByDescending(p => p[1]).Take(k).ToList();
        }
    }

    public class ClinicManagerStub
    {
        static int passed = 0, failed = 0, skipped = 0;

        public static void Main(string[] args)
        {
            // <bug-task1>
            RunTest("testGetAppointmentStatistics", TestGetAppointmentStatistics);
            // </bug-task1>

            // <task2>
            RunTest("testGetAverageAppointmentDurationByType",TestGetAverageAppointmentDurationByType);
            // </task2>

            // <task3>
            RunTest("testGetDoctorAppointmentSummary",TestGetDoctorAppointmentSummary);
            // </task3>

            // <task4>
            RunTest("testGetDoctorWorkloadRankingCase1",TestGetDoctorWorkloadRankingCase1);
            RunTest("testGetDoctorWorkloadRankingCase2", TestGetDoctorWorkloadRankingCase2);
            // </task4>

            Console.WriteLine("\nResults: " + passed + " passed, " + failed + " failed, " + skipped + " skipped");
        }

        // <bug-task1>
        static void TestGetAppointmentStatistics()
        {
            ClinicManager cm = new ClinicManager();
            cm.AddDoctor(new Doctor(10, "dr_smith"));

            cm.AddAppointment(new Appointment(1, 10, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 10, 101, 45, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(3, 10, 102, 30, AppointmentStatus.NO_SHOW, AppointmentType.EMERGENCY));
            cm.AddAppointment(new Appointment(4, 10, 103, 60, AppointmentStatus.CANCELLED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(5, 10, 104, 30, AppointmentStatus.SCHEDULED, AppointmentType.FOLLOWUP));

            AppointmentStats stats = cm.GetAppointmentStatistics();
            AssertEquals(5, stats.totalAppointments);
            AssertEquals(2, stats.completedAppointments);
            AssertEquals(0.2, stats.noShowRate, 1e-4);
        }
        // </bug-task1>

        // <task2>
        static void TestGetAverageAppointmentDurationByType()
        {
            ClinicManager cm = new ClinicManager();
            cm.AddDoctor(new Doctor(1, "dr_smith"));
            cm.AddDoctor(new Doctor(2, "dr_jones"));
            cm.AddDoctor(new Doctor(3, "dr_brown"));
            cm.AddDoctor(new Doctor(4, "dr_lee"));
            cm.AddDoctor(new Doctor(5, "dr_kim"));

            cm.AddAppointment(new Appointment(1, 1, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 1, 101, 41, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(3, 1, 102, 20, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            // not COMPLETED -> excluded from averages
            cm.AddAppointment(new Appointment(5, 1, 105, 100, AppointmentStatus.CANCELLED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(4, 2, 103, 40, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));
            cm.AddAppointment(new Appointment(6, 4, 106, 25, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(7, 5, 107, 15, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));

            Dictionary<AppointmentType, double> avg1 = cm.GetAverageAppointmentDurationByType(1);
            AssertEquals(35.5, avg1[AppointmentType.CONSULTATION], 1e-4); // (30+41)/2
            AssertEquals(20.0, avg1[AppointmentType.FOLLOWUP], 1e-4);
            AssertFalse(avg1.ContainsKey(AppointmentType.EMERGENCY));

            Dictionary<AppointmentType, double> avg2 = cm.GetAverageAppointmentDurationByType(2);
            AssertEquals(40.0, avg2[AppointmentType.EMERGENCY], 1e-4);
            AssertFalse(avg2.ContainsKey(AppointmentType.CONSULTATION));
            AssertFalse(avg2.ContainsKey(AppointmentType.FOLLOWUP));

            Dictionary<AppointmentType, double> avg4 = cm.GetAverageAppointmentDurationByType(4);
            AssertEquals(25.0, avg4[AppointmentType.CONSULTATION], 1e-4);
            AssertFalse(avg4.ContainsKey(AppointmentType.FOLLOWUP));
            AssertFalse(avg4.ContainsKey(AppointmentType.EMERGENCY));

            Dictionary<AppointmentType, double> avg5 = cm.GetAverageAppointmentDurationByType(5);
            AssertEquals(15.0, avg5[AppointmentType.FOLLOWUP], 1e-4);
            AssertFalse(avg5.ContainsKey(AppointmentType.CONSULTATION));
            AssertFalse(avg5.ContainsKey(AppointmentType.EMERGENCY));

            // doctor with no appointments
            AssertTrue(cm.GetAverageAppointmentDurationByType(3).Count == 0);

            // unknown doctorId -> empty map
            AssertTrue(cm.GetAverageAppointmentDurationByType(999).Count == 0);
        }
        // </task2>

        // <task3>
        static void TestGetDoctorAppointmentSummary()
        {
            ClinicManager cm = new ClinicManager();
            foreach (int did in new List<int> { 1, 2, 3, 4 })
            {
                cm.AddDoctor(new Doctor(did, "doctor" + did));
            }

            // doctor 1: 4 appointments, 125 total minutes
            // CONSULTATION x2, FOLLOWUP x1, EMERGENCY x1 -> busiest = CONSULTATION
            cm.AddAppointment(new Appointment(1, 1, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 1, 101, 45, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(3, 1, 102, 20, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(4, 1, 103, 30, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            // doctor 2: 2 appointments, 90 total minutes
            // FOLLOWUP x1, EMERGENCY x1 — tied, tiebreak alphabetically -> EMERGENCY (E < F)
            cm.AddAppointment(new Appointment(5, 2, 104, 40, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(6, 2, 105, 50, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            // doctor 3: no appointments -> zeroed summary

            // doctor 4: 2 appointments, CONSULTATION x1, FOLLOWUP x1 — tied
            // tiebreak alphabetically -> CONSULTATION (C < F)
            cm.AddAppointment(new Appointment(7, 4, 106, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(8, 4, 107, 30, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));

            Dictionary<int, DoctorAppointmentSummary> summary = cm.GetDoctorAppointmentSummary();

            // doctor 1
            AssertEquals(4, summary[1].totalAppointments);
            AssertEquals(125, summary[1].totalMinutes);
            AssertEquals<AppointmentType?>(AppointmentType.CONSULTATION, summary[1].busiestAppointmentType);

            // doctor 2 — tiebreak -> EMERGENCY
            AssertEquals(2, summary[2].totalAppointments);
            AssertEquals(90, summary[2].totalMinutes);
            AssertEquals<AppointmentType?>(AppointmentType.EMERGENCY, summary[2].busiestAppointmentType);

            // doctor 3 — zero appointments
            AssertEquals(0, summary[3].totalAppointments);
            AssertEquals(0, summary[3].totalMinutes);
            AssertNull(summary[3].busiestAppointmentType);

            // doctor 4 — tiebreak -> CONSULTATION
            AssertEquals(2, summary[4].totalAppointments);
            AssertEquals(60, summary[4].totalMinutes);
            AssertEquals<AppointmentType?>(AppointmentType.CONSULTATION, summary[4].busiestAppointmentType);
        }
        // </task3>

        // <task4>
        static void TestGetDoctorWorkloadRankingCase1()
        {
            ClinicManager cm = new ClinicManager();
            foreach (int did in new List<int> { 1, 2, 3 })
            {
                cm.AddDoctor(new Doctor(did, "doctor" + did));
            }

            // doctor 1: 3 appointments, 110 total minutes
            // patient 100 has 2 appointments -> most frequent = 100
            cm.AddAppointment(new Appointment(1, 1, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 1, 100, 50, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(3, 1, 101, 30, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            // doctor 2: 2 appointments, 70 total minutes
            // patient 102 and 103 each have 1 appointment -> tiebreak by patientId -> 102
            cm.AddAppointment(new Appointment(4, 2, 102, 40, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(5, 2, 103, 30, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));

            // doctor 3: no appointments -> excluded
            List<int[]> result = cm.GetDoctorWorkloadRanking(3);
            AssertEquals(2, result.Count);
            AssertArrayEquals(new int[] { 1, 110, 100 }, result[0]);
            AssertArrayEquals(new int[] { 2, 70, 102 }, result[1]);

            // k=0 -> empty
            AssertEquals(0, cm.GetDoctorWorkloadRanking(0).Count);
        }

        static void TestGetDoctorWorkloadRankingCase2()
        {
            ClinicManager cm = new ClinicManager();
            foreach (int did in new List<int> { 1, 2, 3, 4 })
            {
                cm.AddDoctor(new Doctor(did, "doctor" + did));
            }

            // doctor 1: 3 appointments, 90 total minutes; patient 100 x2 -> most frequent = 100
            cm.AddAppointment(new Appointment(1, 1, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 1, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(3, 1, 101, 30, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            // doctor 2: 3 appointments, 90 total minutes (tied with doctor 1)
            // patient 102 x2 -> most frequent = 102; doctor_id tiebreak -> doctor 1 ranks first
            cm.AddAppointment(new Appointment(4, 2, 102, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(5, 2, 102, 30, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(6, 2, 103, 30, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            // doctor 3: 2 appointments, 90 total minutes (tied); patient 104 and 105 tied -> 104
            cm.AddAppointment(new Appointment(7, 3, 104, 45, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(8, 3, 105, 45, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));

            // doctor 4: 1 appointment, 30 total minutes -> ranks last
            cm.AddAppointment(new Appointment(9, 4, 106, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));

            // k=3 -> top 3 only, doctor 4 excluded
            List<int[]> result = cm.GetDoctorWorkloadRanking(3);
            AssertEquals(3, result.Count);
            AssertArrayEquals(new int[] { 1, 90, 100 }, result[0]);
            AssertArrayEquals(new int[] { 2, 90, 102 }, result[1]);
            AssertArrayEquals(new int[] { 3, 90, 104 }, result[2]);
        }
        // </task4>

        // ---- assertion helpers (equivalents of org.junit.jupiter.api.Assertions) ----

        static void AssertEquals(int expected, int actual)
        {
            if (expected != actual) throw new Exception("Expected " + expected + " but got " + actual);
        }

        static void AssertEquals(double expected, double actual, double delta)
        {
            if (Math.Abs(expected - actual) > delta)
                throw new Exception("Expected " + expected + " but got " + actual + " (delta " + delta + ")");
        }

        static void AssertEquals<T>(T expected, T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception("Expected " + expected + " but got " + actual);
        }

        static void AssertTrue(bool condition)
        {
            if (!condition) throw new Exception("Expected condition to be true");
        }

        static void AssertFalse(bool condition)
        {
            if (condition) throw new Exception("Expected condition to be false");
        }

        static void AssertNull(object value)
        {
            if (value != null) throw new Exception("Expected null but got " + value);
        }

        static void AssertArrayEquals(int[] expected, int[] actual)
        {
            if (expected.Length != actual.Length || !expected.SequenceEqual(actual))
                throw new Exception("Expected [" + string.Join(",", expected) + "] but got [" + string.Join(",", actual) + "]");
        }

        // ---- test runner helpers (check()/run() equivalents) ----

        static void RunTest(string name, Action test)
        {
            try
            {
                test();
                passed++;
                Console.WriteLine("PASS: " + name);
            }
            catch (Exception e)
            {
                failed++;
                Console.WriteLine("FAIL: " + name);
                Console.WriteLine("      Error Detail: " + e.Message);
            }
        }

        // Mirrors the Java @Disabled annotation: the test body is still compiled
        // (see the corresponding Test... method above) but is not executed.
        static void SkipTest(string name)
        {
            skipped++;
            Console.WriteLine("SKIPPED (Disabled): " + name);
        }
    }
}
