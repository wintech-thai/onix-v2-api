#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
imageFile = "nap-logo.png"

apiGetPresignUrl = "api/Organization/org/#{orgId}/action/GetLogoImageUploadPresignedUrl"
result = make_request(:get, apiGetPresignUrl, nil)

imagePath = result["imagePath"]
presignedUrl = result["presignedUrl"]
previewUrl = result["previewUrl"]

### Upload file to GCS
puts("Image path  : [#{imagePath}]")
upload_file_to_gcs(presignedUrl, imageFile, 'image/png')

param =  {
  LogoImagePath: imagePath,
  OrgName: "NAP BIOTEC",
  OrgDescription: "NAP Biotec Co., Ltd.",
  Tags: "NAP,Biotec",
  ChannelsArray: [
    { Name: "Company Website", Value: "https://napbiotec.io" },
  ],
  AddressesArray: [
    { Name: "Default Address", Value: "77/15 Moo 7, Khlong Yong Subdistrict, Phutthamonthon District, Nakhon Pathom 73170, Thailand" },
  ],
}

### UpdateUserById
apiUrl = "api/Organization/org/#{orgId}/action/UpdateOrganization"
result = make_request(:post, apiUrl, param)

puts(result)
