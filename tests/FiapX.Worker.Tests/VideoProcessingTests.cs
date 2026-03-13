using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FluentAssertions;
using System;
using Xunit;

namespace FiapX.Worker.Tests;

public class VideoEntityTests
{
    [Fact]
    public void Video_ShouldCreateWithPendingStatus()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "/path/test.mp4");
        video.Status.Should().Be(VideoStatus.Pending);
    }

    [Fact]
    public void Video_StartProcessing_ShouldChangeStatus()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "/path/test.mp4");
        video.StartProcessing();
        video.Status.Should().Be(VideoStatus.Processing);
    }

    [Fact]
    public void Video_CompleteProcessing_ShouldSetFrameCountAndZipPath()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "/path/test.mp4");
        video.CompleteProcessing(100, "/path/output.zip");
        
        video.Status.Should().Be(VideoStatus.Completed);
        video.FrameCount.Should().Be(100);
        video.ZipPath.Should().Be("/path/output.zip");
        video.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Video_FailProcessing_ShouldTruncateErrorMessage()
    {
        var video = new Video(Guid.NewGuid(), "test.mp4", "/path/test.mp4");
        var longError = new string('x', 3000);
        
        video.FailProcessing(longError);
        
        video.Status.Should().Be(VideoStatus.Failed);
        video.ErrorMessage.Should().HaveLength(2000);
    }

    [Fact]
    public void Video_ShouldThrowIfUserIdEmpty()
    {
        Action act = () => new Video(Guid.Empty, "test.mp4", "/path/test.mp4");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Video_ShouldThrowIfFileNameEmpty()
    {
        Action act = () => new Video(Guid.NewGuid(), "", "/path/test.mp4");
        act.Should().Throw<ArgumentNullException>();
    }
}

public class UserEntityTests
{
    [Fact]
    public void User_ShouldCreateWithValidData()
    {
        var user = new User("Test User", "test@test.com", "hashedpassword");
        
        user.Name.Should().Be("Test User");
        user.Email.Should().Be("test@test.com");
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void User_ShouldThrowIfNameEmpty()
    {
        Action act = () => new User("", "test@test.com", "hash");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void User_ShouldThrowIfEmailEmpty()
    {
        Action act = () => new User("Test", "", "hash");
        act.Should().Throw<ArgumentNullException>();
    }
}
