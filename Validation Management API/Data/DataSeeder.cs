using System.Text;
using Microsoft.EntityFrameworkCore;
using Validation_Management_API.Models;
using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Data
{
    // Fills an empty database with predictable test data.
    // Naming rule: "<Thing> N", so Employee 3 is always the third employee, and so on.
    // Safe to call on every startup: it does nothing if data already exists.
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.Departments.AnyAsync()) return;

            // .Date keeps Kind = Utc, which Npgsql requires for timestamptz columns.
            var today = DateTime.UtcNow.Date;

            // ---------- Reference data ----------
            var departments = Enumerable.Range(1, 5)
                .Select(i => new Department { Name = $"Department {i}" }).ToList();

            var locations = Enumerable.Range(1, 4)
                .Select(i => new Location { Name = $"Location {i}" }).ToList();

            // Employee 8 is inactive, to test "deactivated people can't be picked".
            var employees = Enumerable.Range(1, 8)
                .Select(i => new Employee { Name = $"Employee {i}", IsActive = i != 8 }).ToList();
            var activeEmployees = employees.Where(e => e.IsActive).ToList();

            // ---------- Document catalog ----------
            DocumentList Doc(string name, int days, int authors, int reviewers, int approvers,
                             params DocumentList[] dependsOn) => new()
            {
                Name = name,
                Duration = days,
                AuthorCount = authors,
                ReviewerCount = reviewers,
                ApproverCount = approvers,
                DependsOn = dependsOn.ToList()
            };

            var doc1 = Doc("Document 1", 3, 1, 1, 1);
            var doc2 = Doc("Document 2", 5, 1, 1, 1);
            var doc3 = Doc("Document 3", 7, 1, 1, 1, doc2);
            var doc4 = Doc("Document 4", 4, 1, 1, 1, doc2);
            var doc5 = Doc("Document 5", 5, 1, 2, 1, doc3, doc1);
            var doc6 = Doc("Document 6", 6, 1, 2, 1, doc5, doc4);
            var doc7 = Doc("Document 7", 6, 1, 2, 1, doc6);
            var doc8 = Doc("Document 8", 3, 1, 1, 1, doc7);
            var catalog = new[] { doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8 };

            // ---------- Systems ----------
            // System N uses Department/Location/Employee numbers that cycle, so it is easy to predict.
            // System 8 is inactive.
            var systems = Enumerable.Range(1, 8).Select(i => new ComputerSystem
            {
                Name = $"System {i}",
                Code = $"SYS-{i:000}",
                Department = departments[(i - 1) % 5],
                Location = locations[(i - 1) % 4],
                Owner = employees[(i - 1) % 7],
                IsActive = i != 8,
                CreatedAt = today.AddDays(-120 + i)
            }).ToList();

            // ---------- Projects ----------
            var project1 = new Project
            {
                Name = "Project 1",
                Manager = employees[0],
                ValidationType = ProjectValidationType.Validation,
                Status = ProjectStatus.InProgress,
                PlannedStartDate = today.AddDays(-20),
                PlannedEndDate = today.AddDays(40),
                CreatedAt = today.AddDays(-25),
                Systems = [systems[0], systems[5]],
                Locations = [locations[2], locations[1]]
            };

            var project2 = new Project
            {
                Name = "Project 2",
                Manager = employees[1],
                ValidationType = ProjectValidationType.Revalidation,
                Status = ProjectStatus.Completed,
                PlannedStartDate = today.AddDays(-90),
                PlannedEndDate = today.AddDays(-30),
                CreatedAt = today.AddDays(-95),
                CompletedAt = today.AddDays(-32),
                Systems = [systems[1]],
                Locations = [locations[2]]
            };

            var project3 = new Project
            {
                Name = "Project 3",
                Manager = employees[2],
                ValidationType = ProjectValidationType.PeriodicReview,
                Status = ProjectStatus.Planned,
                PlannedStartDate = today.AddDays(5),
                PlannedEndDate = today.AddDays(60),
                CreatedAt = today.AddDays(-2),
                Systems = [systems[2]],
                Locations = [locations[0]]
            };

            var project4 = new Project
            {
                Name = "Project 4",
                Manager = employees[3],
                ValidationType = ProjectValidationType.Addendum,
                Status = ProjectStatus.Cancelled,
                PlannedStartDate = today.AddDays(-60),
                PlannedEndDate = today.AddDays(-10),
                CreatedAt = today.AddDays(-65),
                Systems = [systems[4]],
                Locations = [locations[2]]
            };

            // ---------- Plan builder ----------
            const ProjectTaskStatus Done = ProjectTaskStatus.Completed;
            const ProjectTaskStatus Doing = ProjectTaskStatus.InProgress;
            const ProjectTaskStatus Todo = ProjectTaskStatus.NotStarted;

            var contributorCursor = 0;

            // Adds a document to a project and creates its tasks from the catalog counts.
            // Tasks split the document's window into three stages: Author, Reviewer, Approver.
            // IsOverdue follows the same rule the daily job will use: past end date and not completed.
            ProjectDocument AddDocument(Project project, DocumentList template, int startOffset,
                ProjectTaskStatus author, ProjectTaskStatus reviewer, ProjectTaskStatus approver,
                bool staffed = true)
            {
                var start = today.AddDays(startOffset);
                var end = start.AddDays(template.Duration);
                var stage = Math.Max(1, template.Duration / 3);

                var doc = new ProjectDocument
                {
                    DocumentList = template,
                    PlannedStartDate = start,
                    PlannedEndDate = end
                };

                void AddTasks(TaskType type, int count, ProjectTaskStatus status, int stageIndex)
                {
                    for (var number = 1; number <= count; number++)
                    {
                        var taskStart = start.AddDays(stage * stageIndex);
                        var taskEnd = stageIndex == 2 ? end : start.AddDays(stage * (stageIndex + 1));

                        var task = new ProjectTask
                        {
                            Type = type,
                            Number = number,
                            PlannedStartDate = taskStart,
                            PlannedEndDate = taskEnd,
                            Status = status,
                            IsOverdue = status != ProjectTaskStatus.Completed && taskEnd < today
                        };

                        if (staffed)
                        {
                            // One mandatory contributor per task; reviewers also get an optional one.
                            task.Contributors.Add(new Contributor
                            {
                                Employee = activeEmployees[contributorCursor++ % activeEmployees.Count],
                                IsMandatory = true
                            });
                            if (type == TaskType.Reviewer)
                            {
                                task.Contributors.Add(new Contributor
                                {
                                    Employee = activeEmployees[contributorCursor++ % activeEmployees.Count],
                                    IsMandatory = false
                                });
                            }
                        }

                        doc.Tasks.Add(task);
                    }
                }

                AddTasks(TaskType.Author, template.AuthorCount, author, 0);
                AddTasks(TaskType.Reviewer, template.ReviewerCount, reviewer, 1);
                AddTasks(TaskType.Approver, template.ApproverCount, approver, 2);

                project.ProjectDocuments.Add(doc);
                return doc;
            }

            // Links documents inside one project using the catalog's dependency rules.
            // Only documents that are actually in the project get linked.
            void LinkDependencies(Project project)
            {
                foreach (var doc in project.ProjectDocuments)
                    foreach (var rule in doc.DocumentList.DependsOn)
                    {
                        var target = project.ProjectDocuments.FirstOrDefault(d => d.DocumentList == rule);
                        if (target != null) doc.DependsOn.Add(target);
                    }
            }

            // Project 1 (in progress): Documents 1 and 2 done, Document 3 mid-review (reviewer overdue),
            // Document 4 just started, Document 5 waiting on Document 3.
            AddDocument(project1, doc1, -20, Done, Done, Done);
            AddDocument(project1, doc2, -17, Done, Done, Done);
            AddDocument(project1, doc3, -5, Done, Doing, Todo);
            AddDocument(project1, doc4, 0, Doing, Todo, Todo);
            AddDocument(project1, doc5, 5, Todo, Todo, Todo);
            LinkDependencies(project1);

            // Project 2 (completed): everything finished.
            AddDocument(project2, doc1, -90, Done, Done, Done);
            AddDocument(project2, doc5, -85, Done, Done, Done);
            AddDocument(project2, doc6, -78, Done, Done, Done);
            AddDocument(project2, doc7, -70, Done, Done, Done);
            AddDocument(project2, doc8, -62, Done, Done, Done);
            LinkDependencies(project2);

            // Project 3 (planned): nothing started; Document 2's tasks have no contributors yet.
            AddDocument(project3, doc1, 5, Todo, Todo, Todo);
            AddDocument(project3, doc2, 8, Todo, Todo, Todo, staffed: false);
            LinkDependencies(project3);

            // Project 4 (cancelled): no documents.

            // ---------- Validation history ----------
            void AddHistory(ComputerSystem system, ValidationStatus status, int daysAgo, Project? project = null)
                => system.History!.Add(new ValidationHistory
                {
                    Status = status,
                    ChangedAt = today.AddDays(-daysAgo),
                    Project = project
                });

            // Every system starts as Pending when it is created.
            // Dated to match the system's own CreatedAt.
            foreach (var system in systems)
                AddHistory(system, ValidationStatus.Pending, (int)(today - system.CreatedAt).TotalDays);

            // Systems 1 and 6 are in Project 1 (in progress).
            foreach (var system in new[] { systems[0], systems[5] })
            {
                AddHistory(system, ValidationStatus.Planning, 25, project1);
                AddHistory(system, ValidationStatus.InProgress, 20, project1);
            }

            // System 2 finished Project 2.
            AddHistory(systems[1], ValidationStatus.Planning, 95, project2);
            AddHistory(systems[1], ValidationStatus.InProgress, 90, project2);
            AddHistory(systems[1], ValidationStatus.Completed, 32, project2);

            // System 3 is planned in Project 3.
            AddHistory(systems[2], ValidationStatus.Planning, 2, project3);

            // System 5 was planned in Project 4, which was then cancelled.
            AddHistory(systems[4], ValidationStatus.Planning, 65, project4);

            // ---------- Attachments ----------
            Attachment Note(string fileName, string text)
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                return new Attachment
                {
                    OriginalName = fileName,
                    ContentType = "text/plain",
                    ContentSize = bytes.Length,
                    Content = bytes
                };
            }

            systems[0].Attachments!.Add(Note("Attachment 1.txt", "Test attachment one for System 1."));
            systems[0].Attachments!.Add(Note("Attachment 2.txt", "Test attachment two for System 1."));

            // ---------- Save everything in one transaction ----------
            db.AddRange(departments);
            db.AddRange(locations);
            db.AddRange(employees);
            db.AddRange(catalog);
            db.AddRange(systems);
            db.AddRange(project1, project2, project3, project4);

            await db.SaveChangesAsync();
        }
    }
}
