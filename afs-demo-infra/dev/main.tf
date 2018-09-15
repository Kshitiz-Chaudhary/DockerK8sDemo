provider "azurerm" {
    version = "=1.5.0"
}

terraform {
    backend "azurerm" {}
}

resource "azurerm_resource_group" "k8s" {
    name     = "${var.resource_group_name}"
    location = "${var.location}"
}