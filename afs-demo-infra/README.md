# afs-demo-infra

## Environments

- dev
- qa
- prod

## Tearraform files

- main.tf - Terraform configuration file that declares the Azure provider
- k8s.tf - Terraform configuration file that declares the resources for the Kubernetes cluster
- variables.tf - Terraform variables file
- output.tf - Terraform output file

## Additional links

- https://docs.microsoft.com/en-us/azure/terraform/terraform-create-k8s-cluster-with-tf-and-aks
- https://www.hashicorp.com/blog/kubernetes-cluster-with-aks-and-terraform

## temp

set ARM_SUBSCRIPTION_ID=xxx
set ARM_CLIENT_ID=xxx
set ARM_CLIENT_SECRET=xxx
set ARM_TENANT_ID=xxx

az role assignment create --assignee %ARM_CLIENT_ID% --role Reader
az role assignment create --assignee %ARM_CLIENT_ID% --role Contributor
az login --service-principal --username %ARM_CLIENT_ID% --password %ARM_CLIENT_SECRET% --tenant %ARM_TENANT_ID%

terraform plan -out out.plan -var "client_id=%ARM_CLIENT_ID%" -var "client_secret=%ARM_CLIENT_SECRET%"