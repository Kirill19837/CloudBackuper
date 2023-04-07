terraform {
    required_providers {
      aws = {
        source  = "hashicorp/aws"
        version = "~> 3.27"
      }
    }
  
    required_version = ">= 0.14.9"
  }
  
provider "aws" {
    access_key = "ACCESS KEY" #Paste admin access key here
    secret_key = "SECRET KEY" #Paste admin secret key here
    region = "eu-north-1"
}

resource "aws_s3_bucket" "new-bucket"{
  bucket = "BUCKET NAME" #Bucket name

  tags = {
    Name = "S3Bucket"
  }
}

#Block public access
resource "aws_s3_bucket_public_access_block" "public_block" {
  bucket = aws_s3_bucket.new-bucket.bucket

  block_public_acls   = true
  block_public_policy = true
  ignore_public_acls = true
  restrict_public_buckets = true
}

#DataSource to generate a policy document
data "aws_iam_policy_document" "base_rw_access" {
  statement {
    principals {
      type = "AWS"
      identifiers = [
        "arn:aws:iam::000000000:user/admin" #Paste user/group arn to add permissions, or *
      ]
	  }

    actions = [
      "s3:PutObject",
      "s3:GetObject",
      "s3:ListBucket"
    ]

    resources = [
      aws_s3_bucket.new-bucket.arn,
      "${aws_s3_bucket.new-bucket.arn}/*",
    ]
  }
}

#Resource to add bucket policy to a bucket 
resource "aws_s3_bucket_policy" "base_rw_access" {
  bucket = aws_s3_bucket.new-bucket.id
  policy = data.aws_iam_policy_document.base_rw_access.json
}

