#https://www.terraform.io/docs/providers/azurerm/r/cosmosdb_account.html

resource "azurerm_cosmosdb_account" "db" {
    name                = "${var.cluster_name}-db"
    location            = "${var.location}"
    resource_group_name = "${var.resource_group_name}"
    offer_type          = "Standard"
    kind                = "MongoDB"

    # https://docs.microsoft.com/en-us/azure/cosmos-db/consistency-levels
    consistency_policy {
        consistency_level       = "Eventual"
    }

    # no failover
    enable_automatic_failover = false
    
    # failover_priority 0 specifies the primary location
    geo_location {
        location          = "${var.location}"
        failover_priority = 0
    }

    depends_on = ["azurerm_resource_group.k8s"]
}