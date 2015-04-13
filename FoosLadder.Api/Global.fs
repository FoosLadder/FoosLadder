namespace FoosLadder.Api

open System.Web.Mvc
open System.Web.Optimization

type Global() = 
    inherit System.Web.HttpApplication()

    let RegisterBundles(bundles : BundleCollection) = 
        //TODO add compiler IF to deal with scripts for debug and production
        //Note for future reference: .Include takes params string []. In F#, if there is only a single string, you must write it as [|"string"|] as the compiler can't tell which overload you are using,
        bundles.Add(ScriptBundle("~/bundles/extLibs").Include(
                        "~/Scripts/angular.js", 
                        "~/Scripts/angular-route.js", 
                        "~/Scripts/angular-resource.js",
                        "~/Scripts/angular-local-storage.js",
                        "~/Scripts/loading-bar.js"))
        bundles.Add(ScriptBundle("~/bundles/app").Include(
                        "~/Scripts/app/app.js",
                        "~/Scripts/app/services.js",
                        "~/Scripts/app/controllers.js"))
        bundles.Add(StyleBundle("~/css/extLibs").Include([|"~/Content/bootstrap.css"|]))
        bundles.Add(StyleBundle("~/css/app").Include([|"~/Content/app/app.css"|]))

    let RegisterGlobalFilters (filters:GlobalFilterCollection) =
        filters.Add(new HandleErrorAttribute())

    member __.Start() = 
        RegisterGlobalFilters GlobalFilters.Filters
        RegisterBundles BundleTable.Bundles
