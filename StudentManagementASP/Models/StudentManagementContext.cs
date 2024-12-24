using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementASP.Models;

public partial class StudentManagementContext : DbContext
{
    public StudentManagementContext()
    {
    }

    public StudentManagementContext(DbContextOptions<StudentManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Authentication> Authentications { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseClass> CourseClasses { get; set; }

    public virtual DbSet<CourseType> CourseTypes { get; set; }

    public virtual DbSet<Curriculum> Curricula { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Lecturer> Lecturers { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonInfo> LessonInfos { get; set; }

    public virtual DbSet<Major> Majors { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentClass> StudentClasses { get; set; }

    public virtual DbSet<StudentJoinClass> StudentJoinClasses { get; set; }

    public virtual DbSet<StudentJoinLesson> StudentJoinLessons { get; set; }

    public virtual DbSet<StudyYear> StudyYears { get; set; }

    public virtual DbSet<StudyYearDetail> StudyYearDetails { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Ward> Wards { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Authentication>(entity =>
        {
            entity.ToTable("Authentication");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Course");

            entity.Property(e => e.Credits).HasDefaultValue(1);
            entity.Property(e => e.Infomation).HasMaxLength(255);

            entity.HasOne(d => d.Curriculum).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CurriculumId)
                .HasConstraintName("FK_course_curriculum");

            entity.HasOne(d => d.Semester).WithMany(p => p.Courses)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_course_semester");

            entity.HasOne(d => d.Subject).WithMany(p => p.Courses)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK_course_subject");

            entity.HasOne(d => d.Type).WithMany(p => p.Courses)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK_course_course_type");
        });

        modelBuilder.Entity<CourseClass>(entity =>
        {
            entity.ToTable("CourseClass");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.MaxQuantity).HasDefaultValue(10);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.WeakDays).HasMaxLength(255);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseClasses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_course_class_course");

            entity.HasOne(d => d.DefaultRoom).WithMany(p => p.CourseClasses)
                .HasForeignKey(d => d.DefaultRoomId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_course_class_room");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.CourseClasses)
                .HasForeignKey(d => d.LecturerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_course_class_lecturer");

            entity.HasOne(d => d.Semester).WithMany(p => p.CourseClasses)
                .HasForeignKey(d => d.SemesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseClass_Semester");

            entity.HasOne(d => d.StudentClass).WithMany(p => p.CourseClasses)
                .HasForeignKey(d => d.StudentClassId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_CourseClass_StudentClass");

            entity.HasOne(d => d.Subject).WithMany(p => p.CourseClasses)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK_CourseClass_Subject");
        });

        modelBuilder.Entity<CourseType>(entity =>
        {
            entity.ToTable("CourseType");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Curriculum>(entity =>
        {
            entity.ToTable("Curriculum");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Major).WithMany(p => p.Curricula)
                .HasForeignKey(d => d.MajorId)
                .HasConstraintName("FK_curriculum_major");

            entity.HasOne(d => d.StudyYear).WithMany(p => p.Curricula)
                .HasForeignKey(d => d.StudyYearId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_curriculum_study_year");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DateFound).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("District");

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.ProvinceCodeNavigation).WithMany(p => p.Districts)
                .HasForeignKey(d => d.ProvinceCode)
                .HasConstraintName("FK_District_Province");
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.ToTable("Lecturer");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BirthPlace).HasMaxLength(255);
            entity.Property(e => e.DayOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.HiredDate).HasColumnType("datetime");
            entity.Property(e => e.Nation).HasMaxLength(100);
            entity.Property(e => e.NationId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Religion).HasMaxLength(100);
            entity.Property(e => e.Sex).HasMaxLength(10);
            entity.Property(e => e.StreetAddress).HasMaxLength(255);

            entity.HasOne(d => d.Dept).WithMany(p => p.Lecturers)
                .HasForeignKey(d => d.DeptId)
                .HasConstraintName("FK_lecturer_department");

            entity.HasOne(d => d.DistrictCodeNavigation).WithMany(p => p.Lecturers)
                .HasForeignKey(d => d.DistrictCode)
                .HasConstraintName("FK_Lecturer_District");

            entity.HasOne(d => d.ProvinceCodeNavigation).WithMany(p => p.Lecturers)
                .HasForeignKey(d => d.ProvinceCode)
                .HasConstraintName("FK_Lecturer_Province");

            entity.HasOne(d => d.User).WithMany(p => p.Lecturers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Lecturer_User");

            entity.HasOne(d => d.WardCodeNavigation).WithMany(p => p.Lecturers)
                .HasForeignKey(d => d.WardCode)
                .HasConstraintName("FK_Lecturer_Ward");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("Lesson");

            entity.HasOne(d => d.CourseClass).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CourseClassId)
                .HasConstraintName("FK_lesson_course_class");

            entity.HasOne(d => d.EndLessonNavigation).WithMany(p => p.LessonEndLessonNavigations)
                .HasForeignKey(d => d.EndLesson)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lesson_LessonInfo1");

            entity.HasOne(d => d.Room).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_lesson_room");

            entity.HasOne(d => d.StartLessonNavigation).WithMany(p => p.LessonStartLessonNavigations)
                .HasForeignKey(d => d.StartLesson)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lesson_LessonInfo");
        });

        modelBuilder.Entity<LessonInfo>(entity =>
        {
            entity.ToTable("LessonInfo");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.ToTable("Major");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Dept).WithMany(p => p.Majors)
                .HasForeignKey(d => d.DeptId)
                .HasConstraintName("FK_major_department");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("Province");

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Room");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Settlement).HasMaxLength(50);
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_semester");

            entity.ToTable("Semester");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.SchoolYearDetail).WithMany(p => p.Semesters)
                .HasForeignKey(d => d.SchoolYearDetailId)
                .HasConstraintName("FK_semester_study_year_detail");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_student");

            entity.ToTable("Student");

            entity.Property(e => e.Id)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.BirthPlace).HasMaxLength(50);
            entity.Property(e => e.DayOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FaceData).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Nation).HasMaxLength(100);
            entity.Property(e => e.NationId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Religion).HasMaxLength(100);
            entity.Property(e => e.Sex).HasMaxLength(10);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Còn học");
            entity.Property(e => e.StreetAddress).HasMaxLength(255);

            entity.HasOne(d => d.Curriculum).WithMany(p => p.Students)
                .HasForeignKey(d => d.CurriculumId)
                .HasConstraintName("FK_student_curriculum");

            entity.HasOne(d => d.Dept).WithMany(p => p.Students)
                .HasForeignKey(d => d.DeptId)
                .HasConstraintName("FK_student_department");

            entity.HasOne(d => d.DistrictCodeNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.DistrictCode)
                .HasConstraintName("FK_Student_District");

            entity.HasOne(d => d.Major).WithMany(p => p.Students)
                .HasForeignKey(d => d.MajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_student_major");

            entity.HasOne(d => d.ProvinceCodeNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.ProvinceCode)
                .HasConstraintName("FK_Student_Province");

            entity.HasOne(d => d.StudentClass).WithMany(p => p.Students)
                .HasForeignKey(d => d.StudentClassId)
                .HasConstraintName("FK_Student_StudentClass");

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Student_User");

            entity.HasOne(d => d.WardCodeNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.WardCode)
                .HasConstraintName("FK_Student_Ward");
        });

        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.ToTable("StudentClass");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(125);

            entity.HasOne(d => d.Lecturer).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.LecturerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_StudentClass_Lecturer");
        });

        modelBuilder.Entity<StudentJoinClass>(entity =>
        {
            entity.ToTable("StudentJoinClass");

            entity.Property(e => e.DateJoin).HasColumnType("datetime");
            entity.Property(e => e.StudentId)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.CourseClass).WithMany(p => p.StudentJoinClasses)
                .HasForeignKey(d => d.CourseClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_student_join_class_course_class");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentJoinClasses)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_student_join_class_student");
        });

        modelBuilder.Entity<StudentJoinLesson>(entity =>
        {
            entity.ToTable("StudentJoinLesson");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.JoinTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.StudentId)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Lesson).WithMany(p => p.StudentJoinLessons)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("FK_student_join_lesson_lesson");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentJoinLessons)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_student_join_lesson_student");
        });

        modelBuilder.Entity<StudyYear>(entity =>
        {
            entity.ToTable("StudyYear");

            entity.Property(e => e.ExpireDate).HasColumnType("datetime");
            entity.Property(e => e.Number).HasDefaultValue(1);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.EndYear).WithMany(p => p.StudyYearEndYears)
                .HasForeignKey(d => d.EndYearId)
                .HasConstraintName("FK_StudyYear_StudyYearDetail_end");

            entity.HasOne(d => d.StartYear).WithMany(p => p.StudyYearStartYears)
                .HasForeignKey(d => d.StartYearId)
                .HasConstraintName("FK_StudyYear_StudyYearDetail_start");
        });

        modelBuilder.Entity<StudyYearDetail>(entity =>
        {
            entity.ToTable("StudyYearDetail");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subject");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.DayOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Otp)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("OTP");
            entity.Property(e => e.OtplastestSend)
                .HasColumnType("datetime")
                .HasColumnName("OTPLastestSend");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Auth).WithMany(p => p.Users)
                .HasForeignKey(d => d.AuthId)
                .HasConstraintName("FK_User_Authentication");
        });

        modelBuilder.Entity<Ward>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("Ward");

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.DistrictCodeNavigation).WithMany(p => p.Wards)
                .HasForeignKey(d => d.DistrictCode)
                .HasConstraintName("FK_Ward_District");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
