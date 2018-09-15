# AKS
output "client_key" {
    value = "${azurerm_kubernetes_cluster.k8s.kube_config.0.client_key}"
}
output "client_certificate" {
    value = "${azurerm_kubernetes_cluster.k8s.kube_config.0.client_certificate}"
}
output "cluster_ca_certificate" {
    value = "${azurerm_kubernetes_cluster.k8s.kube_config.0.cluster_ca_certificate}"
}
output "cluster_username" {
    value = "${azurerm_kubernetes_cluster.k8s.kube_config.0.username}"
}
output "cluster_password" {
    value = "${azurerm_kubernetes_cluster.k8s.kube_config.0.password}"
}
output "kube_config" {
    value = "${azurerm_kubernetes_cluster.k8s.kube_config_raw}"
}
output "host" {
    value = "${azurerm_kubernetes_cluster.k8s.kube_config.0.host}"
}

# Cosmos DB
output "cosmos-db-id" {
  value = "${azurerm_cosmosdb_account.db.id}"
}
output "cosmos-db-endpoint" {
  value = "${azurerm_cosmosdb_account.db.endpoint}"
}
output "cosmos-db-endpoints_read" {
  value = "${azurerm_cosmosdb_account.db.read_endpoints}"
}
output "cosmos-db-endpoints_write" {
  value = "${azurerm_cosmosdb_account.db.write_endpoints}"
}
output "cosmos-db-primary_master_key" {
  value = "${azurerm_cosmosdb_account.db.primary_master_key}"
}
output "cosmos-db-secondary_master_key" {
  value = "${azurerm_cosmosdb_account.db.secondary_master_key}"
}