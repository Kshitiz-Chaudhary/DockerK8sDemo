variable "client_id" {}
variable "client_secret" {}

variable "agent_count" {
    default = 1
}

variable "ssh_public_key" {
    default = "~/.ssh/id_rsa.pub"
}

# name in $proj-$participant-$env format
# each participant must change this name in a git branch
variable "dns_prefix" {
    default = "afsdemo-gurba-dev"
}

variable cluster_name {
    default = "afsdemo-gurba-dev"
}

variable resource_group_name {
    default = "afsdemo-gurba-dev"
}

variable location {
    default = "North Europe"
}