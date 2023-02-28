using Microsoft.AspNetCore.Mvc;

namespace Auth.Endpoints
{
    public class TestEndpoints : EndpointsBase
    {


        public IResult GetAsync()
        {

            return Results.Ok();
        }

        public IResult UpdateAsync(string name)
        {
            return Results.Ok();
        }

        public IResult DeleteAsync(int id)
        {
            return Results.Ok();
        }
        public IResult AddAsync([FromBody] Test test)
        {
            return Results.Ok();
        }
    }

    public class Test
    {

        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }
}
