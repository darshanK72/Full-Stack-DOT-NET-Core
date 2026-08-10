/*<bug-task1>
 * We are building a program to manage a clinic's doctor roster and patient visit records.
 * The clinic has multiple doctors, each with a unique ID and name. Patients visit doctors
 * with varying durations and visit types. The program allows clinic staff to add and remove
 * doctors, look up doctors by ID, and retrieve the list of available doctors.
 *
 * Definitions:
 * - A "doctor" is an object that represents a clinic doctor. It has properties for the
 *   doctor ID and doctor name.
 * - A "patient visit" is an object that represents a single visit by a patient to a doctor.
 *   It has properties for the patient ID, doctor ID, visit duration (in minutes), and
 *   visit type. Visit type is one of: SCHEDULED, COMPLETED, or CANCELLED.
 * - A "DoctorManager" is a class used for managing the roster of doctors in the clinic.
 *
 * To begin with, we present you with two tasks:
 * 1-1) Read through and understand the code below. Please take as much time as necessary,
 *      and feel free to run the code.
 * 1-2) The test for DoctorManager is not passing due to a bug in the code. Make the
 *      necessary changes to DoctorManager to fix the bug.
 </bug-task1>
 */

/*<task2>
 * We are updating our system to track patient visits for each doctor.
 * As part of this update, we have introduced the PatientVisit class, which represents
 * a single visit by a patient to a doctor. Each PatientVisit object records the patient ID,
 * the doctor ID, the visit duration in minutes, and the visit type — one of SCHEDULED,
 * COMPLETED, or CANCELLED.
 *
 * To implement these changes, we need to add a method to the DoctorManager class:
 *
 * 2) The addPatientVisit method should record a patient visit against the relevant
 *    doctor. If the given doctor does not exist, the visit can be ignored.
 *
 * To assist you in testing this new method, we have provided the
 * testAddPatientVisit test.
 </task2>
 */

/*<task3>
 * We are now adding analytics on top of the visit records. The clinic wants to understand
 * how much time each doctor spends with patients so they can manage scheduling more
 * effectively.
 *
 * 3) Implement a getTotalVisitDuration method in the DoctorManager class.
 *    This method should return a Map mapping each doctor ID to the total
 *    visit duration (in minutes) across all of their recorded patient visits.
 *    Doctors with no visits should map to 0.
 *
 * To assist you in understanding the requirements and testing this new method, we have
 * provided the testGetTotalVisitDuration test.
 </task3>
 */

/*<task4>
 * We are enhancing our analytics to give clinic management a comprehensive snapshot of each
 * doctor's activity. We have introduced the DoctorSummary class, which captures three key
 * metrics for a doctor: the total number of visits, the total duration of all visits in
 * minutes, and the most frequently occurring visit type.
 *
 * - If a doctor has no visits, their summary should have totalVisits=0, totalDuration=0,
 *   and mostFrequentVisitType=null.
 * - If multiple visit types are tied for most frequent, any one of them may be returned.
 *
 * 4) Implement a getDoctorSummaries method in the DoctorManager class.
 *    This method should return a Map mapping each doctor ID to a DoctorSummary
 *    object computed from their recorded visits.
 *
 * To assist you in understanding the requirements and testing this new method, we have
 * provided the testGetDoctorSummaries test.
 </task4>
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    // Enum & Data Classes
    enum VisitType
    {
        SCHEDULED, COMPLETED, CANCELLED
    }

    class Doctor
    {
        public int doctorId;
        public string doctorName;
        public List<PatientVisit> visits = new List<PatientVisit>();

        public Doctor(int doctorId, string doctorName)
        {
            this.doctorId = doctorId;
            this.doctorName = doctorName;
        }

        public override string ToString()
        {
            return "Doctor(id=" + doctorId + ", name='" + doctorName + "')";
        }
    }

    class PatientVisit
    {
        public int patientId;
        public int doctorId;
        public int visitDuration; // in minutes
        public VisitType visitType;

        public PatientVisit(int patientId, int doctorId, int visitDuration, VisitType visitType)
        {
            this.patientId = patientId;
            this.doctorId = doctorId;
            this.visitDuration = visitDuration;
            this.visitType = visitType;
        }

        public override string ToString()
        {
            return "PatientVisit(patientId=" + patientId + ", doctorId=" + doctorId +
                    ", duration=" + visitDuration + "min, type='" + visitType + "')";
        }
    }

    class DoctorSummary
    {
        public int totalVisits;
        public int totalDuration;
        public VisitType? mostFrequentVisitType;

        public DoctorSummary(int totalVisits, int totalDuration, VisitType? mostFrequentVisitType)
        {
            this.totalVisits = totalVisits;
            this.totalDuration = totalDuration;
            this.mostFrequentVisitType = mostFrequentVisitType;
        }

        public override string ToString()
        {
            return "DoctorSummary(totalVisits=" + totalVisits +
                    ", totalDuration=" + totalDuration + "min" +
                    ", mostFrequentVisitType=" + mostFrequentVisitType + ")";
        }
    }

    // DoctorManager
    class DoctorManager
    {
        public List<Doctor> doctors = new List<Doctor>();

        public void AddDoctor(Doctor doctor)
        {
            doctors.Add(doctor);
        }

        public Doctor GetDoctorById(int doctorId)
        {
            foreach (Doctor doctor in doctors)
            {
                if (doctor.doctorId == doctorId) return doctor;
            }
            return null;
        }

        public void RemoveDoctor(int doctorId)
        {
            Doctor doctor = GetDoctorById(doctorId);
            if (doctor != null)
            {
                doctors.Remove(doctor);
            }
            else
            {
                Console.WriteLine("Doctor with id " + doctorId + " not found.");
            }
        }

        public void ListDoctors()
        {
            if (doctors.Count == 0) { Console.WriteLine("No doctors available."); return; }
            foreach (Doctor doctor in doctors) Console.WriteLine(doctor);
        }

        /**
         * TASK 1-2: This method has a bug. Find and fix it.
         * Should return a count of doctors currently managed.
         */
        public int Size()
        {
            // doctors.Count - 1 is incorrect 
            return doctors.Count;
        }

        /**
         * TASK 2: The addPatientVisit method should record a patient visit against
         * the relevant doctor. If the given doctor does not exist, the visit can
         * be ignored.
         */
        public void AddPatientVisit(int doctorId, PatientVisit visit)
        {
            Doctor doc = GetDoctorById(doctorId);
            if (doc != null)
            {
                doc.visits.Add(visit);
            }
        }

        /**
         * TASK 3: Implement getTotalVisitDuration.
         * This method should return a Map mapping each doctor ID to the total
         * visit duration (in minutes) across all of their recorded patient visits.
         * Doctors with no visits should map to 0.
         */
        public Dictionary<int, int> GetTotalVisitDuration()
        {
            return doctors.ToDictionary(
                    doc => doc.doctorId,
                    doc => doc.visits.Sum(vis => vis.visitDuration)
                );
        }

        /**
         * TASK 4: Implement getDoctorSummaries.
         * This method should return a Map mapping each doctor ID to a DoctorSummary
         * object computed from their recorded visits.
         * - If a doctor has no visits: totalVisits=0, totalDuration=0,
         *   mostFrequentVisitType=null.
         * - If multiple visit types are tied for most frequent, any one may be returned.
         */
        public Dictionary<int, DoctorSummary> GetDoctorSummaries()
        {
            // Dictionary<int, DoctorSummary> output = new Dictionary<int, DoctorSummary>();
            // foreach (Doctor doc in doctors)
            // {
            //     int totalVisits = doc.visits.Count;
            //     if (totalVisits == 0)
            //     {
            //         output[doc.doctorId] = new DoctorSummary(0, 0, null);
            //         continue;
            //     }
            //     int totalDuration = doc.visits.Sum(v => v.visitDuration);

            //     VisitType? mostVisitType = doc.visits
            //                         .GroupBy(v => v.visitType)
            //                         .OrderByDescending(g => g.Count())
            //                         .First()
            //                         .Key;

            //     DoctorSummary docSumm = new DoctorSummary(totalVisits, totalDuration, mostVisitType);

            //     output[doc.doctorId] = docSumm;
            // }
            // return output;
            return doctors.ToDictionary(
                doc => doc.doctorId,
                doc => doc.visits.Count == 0
                    ? new DoctorSummary(0, 0, null)
                    : new DoctorSummary(
                        doc.visits.Count,
                        doc.visits.Sum(v => v.visitDuration),
                        doc.visits
                            .GroupBy(v => v.visitType)
                            .OrderByDescending(g => g.Count())
                            .First()
                            .Key));
        }
    }

    // Test Runner
    public class DoctorAppointmentStub
    {
        static int passed = 0, failed = 0;

        public static void Main(string[] args)
        {
            Console.WriteLine("=== DOCTOR CLINIC SYSTEM TEST SUITE ===\n");

            // <bug-task1>
            RunTest("BUG 1-2: Doctor Manager Size (fix the bug)", () =>
            {
                DoctorManager manager = new DoctorManager();
                manager.AddDoctor(new Doctor(1, "Dr. Smith"));
                manager.AddDoctor(new Doctor(2, "Dr. Jones"));
                manager.AddDoctor(new Doctor(3, "Dr. Patel"));
                manager.AddDoctor(new Doctor(4, "Dr. Lee"));
                manager.AddDoctor(new Doctor(5, "Dr. Kim"));
                Check(manager.Size() == 5, "Expected 5 doctors, but got: " + manager.Size());

                Doctor doc3 = manager.GetDoctorById(3);
                Check(doc3 != null, "getDoctorById(3) should not be null");
                Check(doc3.doctorId == 3, "getDoctorById(3) should return doctor with id 3");
                Check(manager.GetDoctorById(99) == null, "getDoctorById(99) should return null");

                manager.RemoveDoctor(2);
                Check(manager.Size() == 4, "Expected 4 doctors after removal, but got: " + manager.Size());
                Check(manager.GetDoctorById(2) == null, "Dr. Jones should be gone after removal");
            });
            // </bug-task1>

            // <task2>
            RunTest("TASK 2: Add Patient Visit", () =>
            {
                DoctorManager manager = new DoctorManager();
                manager.AddDoctor(new Doctor(1, "Dr. Smith"));
                manager.AddDoctor(new Doctor(2, "Dr. Jones"));
                manager.AddDoctor(new Doctor(3, "Dr. Patel"));

                PatientVisit v1 = new PatientVisit(101, 1, 30, VisitType.COMPLETED);
                PatientVisit v2 = new PatientVisit(102, 1, 45, VisitType.SCHEDULED);
                PatientVisit v3 = new PatientVisit(103, 1, 20, VisitType.CANCELLED);
                PatientVisit v4 = new PatientVisit(104, 2, 60, VisitType.COMPLETED);
                PatientVisit v5 = new PatientVisit(105, 2, 90, VisitType.COMPLETED);
                PatientVisit v6 = new PatientVisit(106, 99, 15, VisitType.SCHEDULED); // doctor 99 does not exist

                manager.AddPatientVisit(1, v1);
                manager.AddPatientVisit(1, v2);
                manager.AddPatientVisit(1, v3);
                manager.AddPatientVisit(2, v4);
                manager.AddPatientVisit(2, v5);
                manager.AddPatientVisit(99, v6); // should be ignored

                Check(manager.GetDoctorById(1).visits.Count == 3, "Task 2: Dr.1 should have 3 visits, got: " + manager.GetDoctorById(1).visits.Count);
                Check(manager.GetDoctorById(2).visits.Count == 2, "Task 2: Dr.2 should have 2 visits, got: " + manager.GetDoctorById(2).visits.Count);
                Check(manager.GetDoctorById(3).visits.Count == 0, "Task 2: Dr.3 should have 0 visits, got: " + manager.GetDoctorById(3).visits.Count);
                Check(manager.GetDoctorById(1).visits.Contains(v1), "Task 2: Dr.1 visits should contain v1");
                Check(manager.GetDoctorById(1).visits.Contains(v2), "Task 2: Dr.1 visits should contain v2");
                Check(manager.GetDoctorById(1).visits.Contains(v3), "Task 2: Dr.1 visits should contain v3");
                Check(manager.GetDoctorById(2).visits.Contains(v4), "Task 2: Dr.2 visits should contain v4");
                Check(manager.GetDoctorById(2).visits.Contains(v5), "Task 2: Dr.2 visits should contain v5");
            });
            // </task2>

            // <task3>
            RunTest("TASK 3: Total Visit Duration Per Doctor", () =>
            {
                DoctorManager manager = new DoctorManager();
                manager.AddDoctor(new Doctor(1, "Dr. Smith"));
                manager.AddDoctor(new Doctor(2, "Dr. Jones"));
                manager.AddDoctor(new Doctor(3, "Dr. Patel"));
                manager.AddDoctor(new Doctor(4, "Dr. Lee"));
                manager.AddDoctor(new Doctor(5, "Dr. Kim"));

                manager.AddPatientVisit(1, new PatientVisit(101, 1, 30, VisitType.COMPLETED));
                manager.AddPatientVisit(1, new PatientVisit(102, 1, 45, VisitType.SCHEDULED));
                manager.AddPatientVisit(1, new PatientVisit(103, 1, 15, VisitType.CANCELLED));
                manager.AddPatientVisit(2, new PatientVisit(104, 2, 60, VisitType.COMPLETED));
                manager.AddPatientVisit(2, new PatientVisit(105, 2, 90, VisitType.COMPLETED));
                manager.AddPatientVisit(3, new PatientVisit(106, 3, 20, VisitType.SCHEDULED));
                // Doctors 4 and 5 get no visits

                Dictionary<int, int> durations = manager.GetTotalVisitDuration();
                Check(GetOrDefault(durations, 1, 0) == 90, "Task 3: Dr.1 total should be 90 (30+45+15), got: " + GetOrDefault(durations, 1, 0));
                Check(GetOrDefault(durations, 2, 0) == 150, "Task 3: Dr.2 total should be 150 (60+90), got: " + GetOrDefault(durations, 2, 0));
                Check(GetOrDefault(durations, 3, 0) == 20, "Task 3: Dr.3 total should be 20, got: " + GetOrDefault(durations, 3, 0));
                Check(GetOrDefault(durations, 4, 0) == 0, "Task 3: Dr.4 total should be 0 (no visits), got: " + GetOrDefault(durations, 4, 0));
                Check(GetOrDefault(durations, 5, 0) == 0, "Task 3: Dr.5 total should be 0 (no visits), got: " + GetOrDefault(durations, 5, 0));
            });
            // </task3>

            // <task4>
            RunTest("TASK 4: Doctor Summaries", () =>
            {
                DoctorManager manager = new DoctorManager();
                manager.AddDoctor(new Doctor(1, "Dr. Smith"));
                manager.AddDoctor(new Doctor(2, "Dr. Jones"));
                manager.AddDoctor(new Doctor(3, "Dr. Patel"));
                manager.AddDoctor(new Doctor(4, "Dr. Lee"));
                manager.AddDoctor(new Doctor(5, "Dr. Kim"));

                manager.AddPatientVisit(1, new PatientVisit(101, 1, 30, VisitType.COMPLETED));
                manager.AddPatientVisit(1, new PatientVisit(102, 1, 45, VisitType.COMPLETED));
                manager.AddPatientVisit(1, new PatientVisit(103, 1, 20, VisitType.SCHEDULED));
                manager.AddPatientVisit(1, new PatientVisit(104, 1, 10, VisitType.CANCELLED));
                manager.AddPatientVisit(2, new PatientVisit(105, 2, 60, VisitType.SCHEDULED));
                manager.AddPatientVisit(2, new PatientVisit(106, 2, 90, VisitType.SCHEDULED));
                manager.AddPatientVisit(2, new PatientVisit(107, 2, 30, VisitType.COMPLETED));
                manager.AddPatientVisit(3, new PatientVisit(108, 3, 40, VisitType.CANCELLED));
                // Doctors 4 and 5 get no visits

                Dictionary<int, DoctorSummary> summaries = manager.GetDoctorSummaries();

                Check(summaries.ContainsKey(1), "Task 4: Dr.1 summary missing from map");
                Check(summaries[1].totalVisits == 4, "Task 4: Dr.1 totalVisits should be 4, got: " + summaries[1].totalVisits);
                Check(summaries[1].totalDuration == 105, "Task 4: Dr.1 totalDuration should be 105, got: " + summaries[1].totalDuration);
                Check(summaries[1].mostFrequentVisitType == VisitType.COMPLETED, "Task 4: Dr.1 mostFrequent should be COMPLETED, got: " + summaries[1].mostFrequentVisitType);

                Check(summaries.ContainsKey(2), "Task 4: Dr.2 summary missing from map");
                Check(summaries[2].totalVisits == 3, "Task 4: Dr.2 totalVisits should be 3, got: " + summaries[2].totalVisits);
                Check(summaries[2].totalDuration == 180, "Task 4: Dr.2 totalDuration should be 180, got: " + summaries[2].totalDuration);
                Check(summaries[2].mostFrequentVisitType == VisitType.SCHEDULED, "Task 4: Dr.2 mostFrequent should be SCHEDULED, got: " + summaries[2].mostFrequentVisitType);

                Check(summaries.ContainsKey(3), "Task 4: Dr.3 summary missing from map");
                Check(summaries[3].totalVisits == 1, "Task 4: Dr.3 totalVisits should be 1, got: " + summaries[3].totalVisits);
                Check(summaries[3].totalDuration == 40, "Task 4: Dr.3 totalDuration should be 40, got: " + summaries[3].totalDuration);
                Check(summaries[3].mostFrequentVisitType == VisitType.CANCELLED, "Task 4: Dr.3 mostFrequent should be CANCELLED, got: " + summaries[3].mostFrequentVisitType);

                Check(summaries.ContainsKey(4), "Task 4: Dr.4 summary missing from map");
                Check(summaries[4].totalVisits == 0, "Task 4: Dr.4 totalVisits should be 0, got: " + summaries[4].totalVisits);
                Check(summaries[4].totalDuration == 0, "Task 4: Dr.4 totalDuration should be 0, got: " + summaries[4].totalDuration);
                Check(summaries[4].mostFrequentVisitType == null, "Task 4: Dr.4 mostFrequent should be null, got: " + summaries[4].mostFrequentVisitType);

                Check(summaries.ContainsKey(5), "Task 4: Dr.5 summary missing from map");
                Check(summaries[5].totalVisits == 0, "Task 4: Dr.5 totalVisits should be 0, got: " + summaries[5].totalVisits);
                Check(summaries[5].totalDuration == 0, "Task 4: Dr.5 totalDuration should be 0, got: " + summaries[5].totalDuration);
                Check(summaries[5].mostFrequentVisitType == null, "Task 4: Dr.5 mostFrequent should be null, got: " + summaries[5].mostFrequentVisitType);
            });
            // </task4>

            Console.WriteLine("\nResults: " + passed + " passed, " + failed + " failed");
        }

        static int GetOrDefault(Dictionary<int, int> map, int key, int defaultValue)
        {
            return map.TryGetValue(key, out int value) ? value : defaultValue;
        }

        static void Check(bool condition, string msg)
        {
            if (!condition) throw new Exception(msg);
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
                Console.WriteLine("-------------------------------------------");
            }
        }
    }
}
