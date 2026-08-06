/*
 * ClinicManager — Complete Solution
 *
 * Task 1: Fix bug in GetAppointmentStatistics (count only COMPLETED appointments)
 * Task 2: GetAverageAppointmentDurationByType
 * Task 3: GetDoctorAppointmentSummary
 * Task 4: GetDoctorWorkloadRanking
 *
 * All tests are enabled in Main() below.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions.ClinicManagerAnswer
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

        // TASK 1 FIX: Only count appointments with status COMPLETED.
        public AppointmentStats GetAppointmentStatistics()
        {
            int total = appointments.Count;

            int completed = 0;
            foreach (Appointment a in appointments)
            {
                if (a.status == AppointmentStatus.COMPLETED)
                    completed++;
            }

            int noShows = 0;
            foreach (Appointment a in appointments)
            {
                if (a.status == AppointmentStatus.NO_SHOW) noShows++;
            }

            double noShowRate = total > 0 ? (double)noShows / total : 0.0;
            return new AppointmentStats(total, completed, noShowRate);
        }

        // TASK 2: Average duration per type for COMPLETED appointments of a doctor.
        public Dictionary<AppointmentType, double> GetAverageAppointmentDurationByType(int doctorId)
        {
            return appointments
                .Where(a => a.doctorId == doctorId && a.status == AppointmentStatus.COMPLETED)
                .GroupBy(a => a.appointmentType)
                .ToDictionary(g => g.Key, g => g.Average(a => (double)a.durationMinutes));
        }

        // TASK 3: Summary for every registered doctor (all appointment statuses).
        public Dictionary<int, DoctorAppointmentSummary> GetDoctorAppointmentSummary()
        {
            var result = new Dictionary<int, DoctorAppointmentSummary>();

            foreach (Doctor doctor in doctors.Values)
            {
                List<Appointment> doctorAppts = appointments
                    .Where(a => a.doctorId == doctor.doctorId)
                    .ToList();

                if (doctorAppts.Count == 0)
                {
                    result[doctor.doctorId] = new DoctorAppointmentSummary(0, 0, null);
                    continue;
                }

                int totalAppts = doctorAppts.Count;
                int totalMins = doctorAppts.Sum(a => a.durationMinutes);

                AppointmentType busiest = doctorAppts
                    .GroupBy(a => a.appointmentType)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key.ToString())
                    .First()
                    .Key;

                result[doctor.doctorId] = new DoctorAppointmentSummary(totalAppts, totalMins, busiest);
            }

            return result;
        }

        // TASK 4: Top-k doctors ranked by total minutes worked.
        public List<int[]> GetDoctorWorkloadRanking(int k)
        {
            if (k <= 0)
                return new List<int[]>();

            return appointments
                .GroupBy(a => a.doctorId)
                .Select(g => new
                {
                    DoctorId = g.Key,
                    TotalMinutes = g.Sum(a => a.durationMinutes),
                    MostFrequentPatientId = g
                        .GroupBy(a => a.patientId)
                        .OrderByDescending(pg => pg.Count())
                        .ThenBy(pg => pg.Key)
                        .First()
                        .Key
                })
                .OrderByDescending(x => x.TotalMinutes)
                .ThenBy(x => x.DoctorId)
                .Take(k)
                .Select(x => new int[] { x.DoctorId, x.TotalMinutes, x.MostFrequentPatientId })
                .ToList();
        }
    }

    public class ClinicManagerAnswerRunner
    {
        static int passed = 0, failed = 0, skipped = 0;

        public static void Main(string[] args)
        {
            RunTest("testGetAppointmentStatistics", TestGetAppointmentStatistics);
            RunTest("testGetAverageAppointmentDurationByType", TestGetAverageAppointmentDurationByType);
            RunTest("testGetDoctorAppointmentSummary", TestGetDoctorAppointmentSummary);
            RunTest("testGetDoctorWorkloadRankingCase1", TestGetDoctorWorkloadRankingCase1);
            RunTest("testGetDoctorWorkloadRankingCase2", TestGetDoctorWorkloadRankingCase2);

            Console.WriteLine("\nResults: " + passed + " passed, " + failed + " failed, " + skipped + " skipped");
        }

        static void TestGetAppointmentStatistics()
        {
            ClinicManager cm = new ClinicManager();
            cm.AddDoctor(new Doctor(10, "dr_smith"));

            cm.AddAppointment(new Appointment(1, 10, 100, 30, AppointmentStatus.COMPLETED,  AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 10, 101, 45, AppointmentStatus.COMPLETED,  AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(3, 10, 102, 30, AppointmentStatus.NO_SHOW,    AppointmentType.EMERGENCY));
            cm.AddAppointment(new Appointment(4, 10, 103, 60, AppointmentStatus.CANCELLED,  AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(5, 10, 104, 30, AppointmentStatus.SCHEDULED,  AppointmentType.FOLLOWUP));

            AppointmentStats stats = cm.GetAppointmentStatistics();
            AssertEquals(5, stats.totalAppointments);
            AssertEquals(2, stats.completedAppointments);
            AssertEquals(0.2, stats.noShowRate, 1e-4);
        }

        static void TestGetAverageAppointmentDurationByType()
        {
            ClinicManager cm = new ClinicManager();
            cm.AddDoctor(new Doctor(1, "dr_smith"));
            cm.AddDoctor(new Doctor(2, "dr_jones"));
            cm.AddDoctor(new Doctor(3, "dr_brown"));
            cm.AddDoctor(new Doctor(4, "dr_lee"));
            cm.AddDoctor(new Doctor(5, "dr_kim"));

            cm.AddAppointment(new Appointment(1, 1, 100, 30,  AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 1, 101, 41,  AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(3, 1, 102, 20,  AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(5, 1, 105, 100, AppointmentStatus.CANCELLED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(4, 2, 103, 40,  AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));
            cm.AddAppointment(new Appointment(6, 4, 106, 25,  AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(7, 5, 107, 15,  AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));

            Dictionary<AppointmentType, double> avg1 = cm.GetAverageAppointmentDurationByType(1);
            AssertEquals(35.5, avg1[AppointmentType.CONSULTATION], 1e-4);
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

            AssertTrue(cm.GetAverageAppointmentDurationByType(3).Count == 0);
            AssertTrue(cm.GetAverageAppointmentDurationByType(999).Count == 0);
        }

        static void TestGetDoctorAppointmentSummary()
        {
            ClinicManager cm = new ClinicManager();
            foreach (int did in new List<int> { 1, 2, 3, 4 })
            {
                cm.AddDoctor(new Doctor(did, "doctor" + did));
            }

            cm.AddAppointment(new Appointment(1, 1, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 1, 101, 45, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(3, 1, 102, 20, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(4, 1, 103, 30, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            cm.AddAppointment(new Appointment(5, 2, 104, 40, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(6, 2, 105, 50, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            cm.AddAppointment(new Appointment(7, 4, 106, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(8, 4, 107, 30, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));

            Dictionary<int, DoctorAppointmentSummary> summary = cm.GetDoctorAppointmentSummary();

            AssertEquals(4, summary[1].totalAppointments);
            AssertEquals(125, summary[1].totalMinutes);
            AssertEquals<AppointmentType?>(AppointmentType.CONSULTATION, summary[1].busiestAppointmentType);

            AssertEquals(2, summary[2].totalAppointments);
            AssertEquals(90, summary[2].totalMinutes);
            AssertEquals<AppointmentType?>(AppointmentType.EMERGENCY, summary[2].busiestAppointmentType);

            AssertEquals(0, summary[3].totalAppointments);
            AssertEquals(0, summary[3].totalMinutes);
            AssertNull(summary[3].busiestAppointmentType);

            AssertEquals(2, summary[4].totalAppointments);
            AssertEquals(60, summary[4].totalMinutes);
            AssertEquals<AppointmentType?>(AppointmentType.CONSULTATION, summary[4].busiestAppointmentType);
        }

        static void TestGetDoctorWorkloadRankingCase1()
        {
            ClinicManager cm = new ClinicManager();
            foreach (int did in new List<int> { 1, 2, 3 })
            {
                cm.AddDoctor(new Doctor(did, "doctor" + did));
            }

            cm.AddAppointment(new Appointment(1, 1, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 1, 100, 50, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(3, 1, 101, 30, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            cm.AddAppointment(new Appointment(4, 2, 102, 40, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(5, 2, 103, 30, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));

            List<int[]> result = cm.GetDoctorWorkloadRanking(3);
            AssertEquals(2, result.Count);
            AssertArrayEquals(new int[] { 1, 110, 100 }, result[0]);
            AssertArrayEquals(new int[] { 2, 70, 102 }, result[1]);

            AssertEquals(0, cm.GetDoctorWorkloadRanking(0).Count);
        }

        static void TestGetDoctorWorkloadRankingCase2()
        {
            ClinicManager cm = new ClinicManager();
            foreach (int did in new List<int> { 1, 2, 3, 4 })
            {
                cm.AddDoctor(new Doctor(did, "doctor" + did));
            }

            cm.AddAppointment(new Appointment(1, 1, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(2, 1, 100, 30, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(3, 1, 101, 30, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            cm.AddAppointment(new Appointment(4, 2, 102, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(5, 2, 102, 30, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));
            cm.AddAppointment(new Appointment(6, 2, 103, 30, AppointmentStatus.COMPLETED, AppointmentType.EMERGENCY));

            cm.AddAppointment(new Appointment(7, 3, 104, 45, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));
            cm.AddAppointment(new Appointment(8, 3, 105, 45, AppointmentStatus.COMPLETED, AppointmentType.FOLLOWUP));

            cm.AddAppointment(new Appointment(9, 4, 106, 30, AppointmentStatus.COMPLETED, AppointmentType.CONSULTATION));

            List<int[]> result = cm.GetDoctorWorkloadRanking(3);
            AssertEquals(3, result.Count);
            AssertArrayEquals(new int[] { 1, 90, 100 }, result[0]);
            AssertArrayEquals(new int[] { 2, 90, 102 }, result[1]);
            AssertArrayEquals(new int[] { 3, 90, 104 }, result[2]);
        }

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
    }
}
