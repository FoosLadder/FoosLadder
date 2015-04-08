namespace FoosLadder.Api

open System.Web.Mvc
open System.Web.Optimization
open System.Web.Http

type Global() = 
    inherit System.Web.HttpApplication()

    let RegisterBundles (bundles : BundleCollection) = 
        //Note for future reference: .Include takes params string []. In F#, if there is only a single string, you must write it as [|"string"|] as the compiler can't tell which overload you are using,
        bundles.Add(ScriptBundle("~/bundles/extLibs").Include(
                        "~/Scripts/angular.js", 
                        "~/Scripts/angular-route.js", 
                        "~/Scripts/angular-resource.js"))
        bundles.Add(ScriptBundle("~/bundles/app").Include(
                        "~/Scripts/app/app.js",
                        "~/Scripts/app/services.js",
                        "~/Scripts/app/controllers.js"))
        bundles.Add(StyleBundle("~/css/extLibs").Include([|"~/Content/bootstrap.css"|]))
        bundles.Add(StyleBundle("~/css/app").Include())

    let RegisterGlobalFilters (filters: GlobalFilterCollection) = 
        filters.Add(new HandleErrorAttribute())
        //filters.Add(new AuthorizeAttribute())
        filters.Add(new RequireHttpsAttribute())

    member __.Start() = 
        //GlobalConfiguration.Configure(WebApiConfig.Register);
        RegisterGlobalFilters GlobalFilters.Filters
        RegisterBundles BundleTable.Bundles
