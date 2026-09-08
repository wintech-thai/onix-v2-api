#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

keyFile = ".token"
fileUpload = 'logo.svg'
mimeType = "image/svg+xml"

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

base64Content = Base64.strict_encode64(File.read(fileUpload))

### No more presigned URL / MinIO upload — send base64 content directly
apiUrl = "admin-api/AdminConfiguration/org/global/action/SetBrandConfig"
param = {
  Status: "Enable",
  BrandConfig: {
    BrandName: "Test Brand Base64",
    LogoBase64: base64Content,
    LogoMimeType: mimeType,
    ThemeName: "GREEN_NATURAL",
  }
}

result = make_request(:post, apiUrl, param)
puts(result.to_json)
