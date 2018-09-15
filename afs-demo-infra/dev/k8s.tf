# https://www.terraform.io/docs/providers/azurerm/r/container_service.html

resource "azurerm_kubernetes_cluster" "k8s" {
    name                = "${var.cluster_name}"
    location            = "${var.location}"
    resource_group_name = "${var.resource_group_name}"
    dns_prefix          = "${var.dns_prefix}"

    linux_profile {
        admin_username = "ubuntu"

        ssh_key {
        key_data = "${file("${var.ssh_public_key}")}"
        }
    }

    # Standard_A1 1 core, 32GB disk- $43.80
    agent_pool_profile {
        name            = "default"
        count           = "${var.agent_count}"
        vm_size         = "Standard_A1"
        os_type         = "Linux"
        os_disk_size_gb = 32
    }

    service_principal {
        client_id     = "${var.client_id}"
        client_secret = "${var.client_secret}"
    }

    tags {
        Environment = "dev"
    }

    depends_on = ["azurerm_resource_group.k8s"]
}