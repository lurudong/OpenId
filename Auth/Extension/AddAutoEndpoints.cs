using Microsoft.Extensions.Options;
using PluralizeService.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Auth.Extension
{
    public static class AddAutoEndpoints
    {



        public static IServiceCollection AddAutoEndpoint(this IServiceCollection services, Action<EndpointOptions> options = null
            , params Type[] types)
        {

            //Action<EndpointOptions> action = null;
            //EndpointOptions options1 = new EndpointOptions();
            //options.Invoke(options1);
            foreach (var type in types)
            {
                services.AddScoped(type);
                //options1.types.Add(type);
                options = o => o.types.Add(type);
            }

            //options1.GetPrefixes?.AddRange(new List<string> { "Get", "Find" });
            //options1.PostPrefixes?.AddRange(new List<string> { "Post", "Add", "Insert" });
            //options1.PutPrefixes?.AddRange(new List<string> { "Put", "Update", "Modify" });
            //options1.DeletePrefixes?.AddRange(new List<string> { "Delete", "Remove" });
            //services.AddSingleton(options1!);

            services.Configure<EndpointOptions>(o =>
            {
                o.GetPrefixes?.AddRange(new List<string> { "Get", "Find" });
                o.PostPrefixes?.AddRange(new List<string> { "Post", "Add", "Insert" });
                o.PutPrefixes?.AddRange(new List<string> { "Put", "Update", "Modify" });
                o.DeletePrefixes?.AddRange(new List<string> { "Delete", "Remove" });
                options(o);
            });

            return services;
        }

        private static string TrimEndMethodName(this string methodName)
    => methodName.TrimEnd("Async", StringComparison.OrdinalIgnoreCase);
        public static IEndpointRouteBuilder MapAutoEndpoin(this IEndpointRouteBuilder endpointRoute)
        {
            using var scope = endpointRoute.ServiceProvider.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<EndpointOptions>>().Value;


            foreach (var type in options.Types)
            {
                var serviceName = type.Name.TrimEnd("Endpoints", StringComparison.OrdinalIgnoreCase).TrimEnd("Service", StringComparison.OrdinalIgnoreCase);

                foreach (var methodInfo in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
                {
                    var servicesType = scope.ServiceProvider.GetService(methodInfo.DeclaringType!);
                    var type1 = Expression.GetDelegateType(
                         methodInfo.GetParameters().

                             Select(parameterInfo => parameterInfo.ParameterType)
                    .Concat(new List<Type>
                        { methodInfo.ReturnType }).ToArray());
                    var instance = Delegate.
                        CreateDelegate(type1, servicesType, methodInfo);



                    var methodName = methodInfo.Name.TrimEndMethodName();

                    endpointRoute.MapMethods($"/api/{PluralizationProvider.Pluralize(serviceName.ToLower())}", new[] { ParseMethod(methodInfo, options) }, instance);
                }
            }

            string ParseMethod(MethodInfo methodInfo, EndpointOptions endpointOptions)
            {

                var methodName = methodInfo.Name;

                if (!string.IsNullOrWhiteSpace(ParseMethodPrefix(endpointOptions.GetPrefixes!, methodName)))
                {
                    return HttpMethods.Get;
                }

                if (!string.IsNullOrWhiteSpace(ParseMethodPrefix(endpointOptions.PostPrefixes!, methodName)))
                {
                    return HttpMethods.Post;
                }



                if (!string.IsNullOrWhiteSpace(ParseMethodPrefix(endpointOptions.PutPrefixes!, methodName)))
                {
                    return HttpMethods.Put;
                }


                if (!string.IsNullOrWhiteSpace(ParseMethodPrefix(endpointOptions.DeletePrefixes!, methodName)))
                {
                    return HttpMethods.Delete;
                }
                return HttpMethods.Get;

            }

            string ParseMethodPrefix(IEnumerable<string> prefixes, string methodName)
            {
                var newMethodName = methodName;
                var prefix = prefixes.FirstOrDefault(prefix => newMethodName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

                if (prefix is not null)
                    return prefix;

                return string.Empty;
            }
            return endpointRoute;
        }


        public static string TrimEnd(this string value,
    string trimParameter,
    StringComparison stringComparison)
        {
            if (!value.EndsWith(trimParameter, stringComparison))
                return value;

            return value.Substring(0, value.Length - trimParameter.Length);
        }





        /// <summary>
        /// 单词变成复数形式
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private static string ToPlural(string word)
        {
            Regex plural1 = new Regex("(?<keep>[^aeiou])y$");
            Regex plural2 = new Regex("(?<keep>[aeiou]y)$");
            Regex plural3 = new Regex("(?<keep>[sxzh])$");
            Regex plural4 = new Regex("(?<keep>[^sxzhy])$");

            if (plural1.IsMatch(word))
                return plural1.Replace(word, "${keep}ies");
            else if (plural2.IsMatch(word))
                return plural2.Replace(word, "${keep}s");
            else if (plural3.IsMatch(word))
                return plural3.Replace(word, "${keep}es");
            else if (plural4.IsMatch(word))
                return plural4.Replace(word, "${keep}s");

            return word;
        }
    }

}

