#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']

param =  {
  FullTextSearch: ""
}

### GetUserCount
apiUrl = "api/OrganizationUser/org/#{orgId}/action/GetUserCount"
result = make_request(:post, apiUrl, param)
puts(result)

### GetUsers
apiUrl = "api/OrganizationUser/org/#{orgId}/action/GetUsers"
result = make_request(:post, apiUrl, param)
puts(result)
