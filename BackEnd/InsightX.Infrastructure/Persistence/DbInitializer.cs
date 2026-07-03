using System;
using System.Linq;
using InsightX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static void SeedData(AppDbContext context)
        {
            context.Database.EnsureCreated();
            EnsurePasswordHashColumn(context);

            const string demoPassword = "Password123!";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(demoPassword);

            if (context.Companies.Any())
            {
                var usersWithoutPassword = context.Users.Where(u => string.IsNullOrEmpty(u.PasswordHash)).ToList();
                foreach (var user in usersWithoutPassword)
                {
                    user.PasswordHash = passwordHash;
                }

                if (usersWithoutPassword.Count > 0)
                {
                    context.SaveChanges();
                }

                return;
            }

            // 1. Seed Company
            var company = new Company
            {
                Name = "InsightX Manufacturing Corp"
            };
            context.Companies.Add(company);
            context.SaveChanges(); // Saves company to generate ID

            // 2. Seed Departments
            var prodDept = new Department { Name = "Production", CompanyId = company.Id };
            var qcDept = new Department { Name = "Quality Control", CompanyId = company.Id };
            var hrDept = new Department { Name = "Human Resources", CompanyId = company.Id };
            
            context.Departments.AddRange(prodDept, qcDept, hrDept);
            context.SaveChanges();

            // 3. Seed Users
            var owner = new User
            {
                Username = "owner",
                Email = "owner@insightx.com",
                PasswordHash = passwordHash,
                Role = "Owner",
                CompanyId = company.Id,
                DepartmentId = null
            };
            
            var prodManager = new User
            {
                Username = "prod_mgr",
                Email = "prod_mgr@insightx.com",
                PasswordHash = passwordHash,
                Role = "Manager",
                CompanyId = company.Id,
                DepartmentId = prodDept.Id
            };

            var qcManager = new User
            {
                Username = "qc_mgr",
                Email = "qc_mgr@insightx.com",
                PasswordHash = passwordHash,
                Role = "Manager",
                CompanyId = company.Id,
                DepartmentId = qcDept.Id
            };

            var hrManager = new User
            {
                Username = "hr_mgr",
                Email = "hr_mgr@insightx.com",
                PasswordHash = passwordHash,
                Role = "Manager",
                CompanyId = company.Id,
                DepartmentId = hrDept.Id
            };

            context.Users.AddRange(owner, prodManager, qcManager, hrManager);
            context.SaveChanges();

            // 4. Seed KPI Thresholds
            var kpiProduction = new KpiThreshold
            {
                CompanyId = company.Id,
                KPIName = "Production",
                ThresholdValue = 1000,
                ComparisonType = "Greater",
                IsPercentage = false
            };

            var kpiDefectRate = new KpiThreshold
            {
                CompanyId = company.Id,
                KPIName = "Defect Rate",
                ThresholdValue = 5.0,
                ComparisonType = "Less",
                IsPercentage = true
            };

            var kpiAbsentEmployees = new KpiThreshold
            {
                CompanyId = company.Id,
                KPIName = "Absent Employees",
                ThresholdValue = 5,
                ComparisonType = "Less",
                IsPercentage = false
            };

            var kpiRevenue = new KpiThreshold
            {
                CompanyId = company.Id,
                KPIName = "Revenue",
                ThresholdValue = 10000,
                ComparisonType = "Greater",
                IsPercentage = false
            };

            context.KpiThresholds.AddRange(kpiProduction, kpiDefectRate, kpiAbsentEmployees, kpiRevenue);
            context.SaveChanges();

            // 5. Seed Reports
            var janReport = new Report
            {
                FileName = "production_report_january_2026.pdf",
                FilePath = "/storage/reports/january_2026.pdf",
                DepartmentId = prodDept.Id,
                UploadedById = prodManager.Id,
                UploadedAt = new DateTime(2026, 1, 31, 17, 0, 0),
                Status = "Done"
            };

            var febReport = new Report
            {
                FileName = "production_report_february_2026.pdf",
                FilePath = "/storage/reports/february_2026.pdf",
                DepartmentId = prodDept.Id,
                UploadedById = prodManager.Id,
                UploadedAt = new DateTime(2026, 2, 28, 17, 0, 0),
                Status = "Done"
            };

            var marReport = new Report
            {
                FileName = "production_report_march_2026.pdf",
                FilePath = "/storage/reports/march_2026.pdf",
                DepartmentId = prodDept.Id,
                UploadedById = prodManager.Id,
                UploadedAt = new DateTime(2026, 3, 31, 17, 0, 0),
                Status = "Done"
            };

            context.Reports.AddRange(janReport, febReport, marReport);
            context.SaveChanges();

            // 6. Seed Extracted Metrics (Jan, Feb, March)
            // January
            context.ExtractedMetrics.AddRange(
                new ExtractedMetric { ReportId = janReport.Id, CompanyId = company.Id, Month = "January", Year = 2026, KPIName = "Production", Value = 1050, ConfirmedByManager = true },
                new ExtractedMetric { ReportId = janReport.Id, CompanyId = company.Id, Month = "January", Year = 2026, KPIName = "Defect Rate", Value = 3.0, ConfirmedByManager = true },
                new ExtractedMetric { ReportId = janReport.Id, CompanyId = company.Id, Month = "January", Year = 2026, KPIName = "Absent Employees", Value = 2, ConfirmedByManager = true },
                new ExtractedMetric { ReportId = janReport.Id, CompanyId = company.Id, Month = "January", Year = 2026, KPIName = "Revenue", Value = 12000, ConfirmedByManager = true }
            );

            // February
            context.ExtractedMetrics.AddRange(
                new ExtractedMetric { ReportId = febReport.Id, CompanyId = company.Id, Month = "February", Year = 2026, KPIName = "Production", Value = 1100, ConfirmedByManager = true },
                new ExtractedMetric { ReportId = febReport.Id, CompanyId = company.Id, Month = "February", Year = 2026, KPIName = "Defect Rate", Value = 2.5, ConfirmedByManager = true },
                new ExtractedMetric { ReportId = febReport.Id, CompanyId = company.Id, Month = "February", Year = 2026, KPIName = "Absent Employees", Value = 3, ConfirmedByManager = true },
                new ExtractedMetric { ReportId = febReport.Id, CompanyId = company.Id, Month = "February", Year = 2026, KPIName = "Revenue", Value = 13000, ConfirmedByManager = true }
            );

            // March (Contains Anomaly)
            context.ExtractedMetrics.AddRange(
                new ExtractedMetric { ReportId = marReport.Id, CompanyId = company.Id, Month = "March", Year = 2026, KPIName = "Production", Value = 600, ConfirmedByManager = true },
                new ExtractedMetric { ReportId = marReport.Id, CompanyId = company.Id, Month = "March", Year = 2026, KPIName = "Defect Rate", Value = 7.0, ConfirmedByManager = true },
                new ExtractedMetric { ReportId = marReport.Id, CompanyId = company.Id, Month = "March", Year = 2026, KPIName = "Absent Employees", Value = 8, ConfirmedByManager = true },
                new ExtractedMetric { ReportId = marReport.Id, CompanyId = company.Id, Month = "March", Year = 2026, KPIName = "Revenue", Value = 7000, ConfirmedByManager = true }
            );

            context.SaveChanges();

            // 7. Seed March Alerts
            context.Alerts.AddRange(
                new Alert
                {
                    CompanyId = company.Id,
                    DepartmentId = prodDept.Id,
                    KPIName = "Production",
                    CurrentValue = 600,
                    Threshold = 1000,
                    Message = "Production dropped 40% in March vs threshold of 1000 units.",
                    Recommendation = "Investigate raw material supply chain and schedule urgent conveyor belt maintenance.",
                    SeenByOwner = false,
                    CreatedAt = new DateTime(2026, 3, 15, 8, 30, 0)
                },
                new Alert
                {
                    CompanyId = company.Id,
                    DepartmentId = qcDept.Id,
                    KPIName = "Defect Rate",
                    CurrentValue = 7.0,
                    Threshold = 5.0,
                    Message = "Defect rate spiked to 7% in March, exceeding the threshold of 5%.",
                    Recommendation = "Recalibrate standard machinery on the main assembly line and check supplier material quality.",
                    SeenByOwner = false,
                    CreatedAt = new DateTime(2026, 3, 18, 9, 15, 0)
                },
                new Alert
                {
                    CompanyId = company.Id,
                    DepartmentId = hrDept.Id,
                    KPIName = "Absent Employees",
                    CurrentValue = 8,
                    Threshold = 5,
                    Message = "Absent employees rose to 8 in March, crossing the threshold of 5.",
                    Recommendation = "Initiate emergency backup staffing plan and check on employee wellness support.",
                    SeenByOwner = false,
                    CreatedAt = new DateTime(2026, 3, 12, 8, 0, 0)
                },
                new Alert
                {
                    CompanyId = company.Id,
                    DepartmentId = prodDept.Id,
                    KPIName = "Revenue",
                    CurrentValue = 7000,
                    Threshold = 10000,
                    Message = "Revenue dropped to $7,000 in March vs threshold of $10,000.",
                    Recommendation = "Analyze production shortfalls and review client contract fulfillment delay terms.",
                    SeenByOwner = false,
                    CreatedAt = new DateTime(2026, 3, 20, 10, 0, 0)
                }
            );

            // 8. Seed Conversation History
            context.Conversations.Add(new Conversation
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                UserId = owner.Id,
                Question = "What was the revenue in January?",
                Answer = "The revenue in January 2026 was $12,000, which exceeded the threshold of $10,000 by $2,000. Operations ran smoothly during that month.",
                CreatedAt = new DateTime(2026, 3, 1, 14, 0, 0)
            });

            context.SaveChanges();
        }

        private static void EnsurePasswordHashColumn(AppDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
                IF COL_LENGTH('Users', 'PasswordHash') IS NULL
                BEGIN
                    ALTER TABLE Users ADD PasswordHash NVARCHAR(200) NOT NULL DEFAULT('');
                END");
        }
    }
}
