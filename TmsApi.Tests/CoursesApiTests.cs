using System.Net;
using System.Net.Http.Json;

namespace TmsApi.Tests;

public class CoursesApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CoursesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCourses_ReturnsOkAndPagedJson()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/v2.0/courses?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();

        var page =
            await response.Content.ReadFromJsonAsync<PagedCoursesJson>();

        Assert.NotNull(page);
        Assert.NotNull(page.Data);
        Assert.NotNull(page.Meta);
        Assert.NotNull(page.Links);
    }

    [Fact]
    public async Task GetCourses_ReturnsExpectedPaginationMetadata()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/v2.0/courses?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();

        var page =
            await response.Content.ReadFromJsonAsync<PagedCoursesJson>();

        Assert.NotNull(page);
        Assert.Equal(1, page.Meta.Page);
        Assert.Equal(10, page.Meta.PageSize);
        Assert.True(page.Meta.TotalCount >= 0);
    }

    private sealed class PagedCoursesJson
    {
        public List<CourseRowJson> Data { get; set; } = [];

        public PaginationMeta Meta { get; set; } = new();

        public List<LinkJson> Links { get; set; } = [];
    }

    private sealed class CourseRowJson
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Title { get; set; } = "";
        public int MaxCapacity { get; set; }
        public int EnrollmentCount { get; set; }
    }

    private sealed class PaginationMeta
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }

    private sealed class LinkJson
    {
        public string Href { get; set; } = "";
        public string Rel { get; set; } = "";
        public string Method { get; set; } = "";
    }
}