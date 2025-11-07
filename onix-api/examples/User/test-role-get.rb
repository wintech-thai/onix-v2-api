#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

param =  {
  FullTextSearch: ""
}

### GetUsers
apiUrl = "api/Role/org/#{orgId}/action/GetRoles"
result = make_request(:post, apiUrl, param)
puts(result)
