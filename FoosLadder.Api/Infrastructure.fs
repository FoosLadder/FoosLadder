namespace FoosLadder.Api

module Infrastructure =

    let GetApplicationSetting (setting : string) =
        let appSetting = System.Configuration.ConfigurationManager.AppSettings
        let value = appSetting.[setting]
        if value = null then None else Some value
