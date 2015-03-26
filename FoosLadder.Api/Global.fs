namespace FoosLadder.Api

open System.Web.Optimization

type BundleConfig() = 
    static member RegisterBundles(bundles : BundleCollection) = 
        bundles.Add(ScriptBundle("~/bundles/extLibs").Include(
                        "~/Scripts/angular.js", 
                        "~/Scripts/angular-route.js", 
                        "~/Scripts/angular-resource.js"))
        bundles.Add(ScriptBundle("~/bundles/app").Include([||]))
        bundles.Add(StyleBundle("~/css/extLibs").Include([|"~/Content/bootstrap.css"|]))
        bundles.Add(StyleBundle("~/css/app").Include([||]))

type Global() = 
    inherit System.Web.HttpApplication()
    member this.Start() = BundleConfig.RegisterBundles BundleTable.Bundles
