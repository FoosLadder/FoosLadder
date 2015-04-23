namespace FoosLadder.Api.FoosDbConfiguration

open System.Data.Entity
open System.Data.Entity.Infrastructure
open System.Data.Entity.SqlServer
open FoosLadder.Api.Context
open FoosLadder.Api.Initializer

type FoosLadderDbConfiguration() as this= 
    inherit DbConfiguration()

    do
        this.SetDefaultConnectionFactory(new SqlConnectionFactory("Data Source=.\sqlexpress;Initial Catalog=FoosLadderAuth;Integrated Security=SSPI;"))
        this.SetProviderServices("System.Data.SqlClient",System.Data.Entity.SqlServer.SqlProviderServices.Instance)
        this.SetDatabaseInitializer<AuthContext>(new AuthInitializer())

