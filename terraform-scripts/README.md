## Preparations

1. Install terraform [Link](https://developer.hashicorp.com/terraform/tutorials/aws-get-started/install-cli?in=terraform%2Faws-get-started)
2. Get user credentials and paste them inside script (L 13-14)
> access_key = "ACCESS KEY"
> secret_key = "SECRET KEY"
3. Rename bucket name to needed (L 19)
> bucket = "BUCKET NAME"
4. Add principals identifiers (L 48)
> identifiers  =  [ ... ]
6. Change allowed actions (L 53 (if needed))
> actions = [ ... ]

## Execute

1. Init terraform 
```sh
terraform init
```
2. Get planned actions
```sh 
terraform plan
```
3. Apply changes
```sh
terraform apply
```