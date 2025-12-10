#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/ScanItemFolder/org/#{orgId}/action/AddScanItemFolder"
param =  {
  FolderName: "XRV-AMX-002",
  Description: "Folder for product XRV-AMX-002",
  ScanItemActionId: "bc6fe784-271d-4c90-89c3-5f356347f066",
  ProductId: "99b5dbd5-7a79-4560-9a89-8c24b0393229",
}
result = make_request(:post, apiUrl, param)
puts(result)
