#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
itemId = "a4688dd4-936d-4931-aef9-898dcd90f9e9"

apiUrl = "api/Privilege/org/#{orgId}/action/AddPrivilegeQuantity/#{itemId}"
param =  {
  TxAmount: 1000,
  Description: "This is product description # 2",
  Tags: "source=test"
}

result = make_request(:post, apiUrl, param)
puts(result)
