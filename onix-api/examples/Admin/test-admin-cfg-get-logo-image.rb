#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

apiBase = ENV['API_HTTP_ENDPOINT']

### GetBrandConfig should now return an <API-BASE>-prefixed LogoImageUrl
apiUrl = "admin-api/AdminConfiguration/org/global/action/GetBrandConfig"
config = make_request(:get, apiUrl, nil)
puts("GetBrandConfig: #{config.to_json}")

logoImageUrl = config.dig("configuration", "brandConfig", "logoImageUrl")
if logoImageUrl.nil?
  puts("No logo configured — nothing to fetch")
  exit
end

actualUrl = logoImageUrl.gsub("<API-BASE>", apiBase)
puts("##### Fetching image from: #{actualUrl}")

uri = URI.parse(actualUrl)
http = Net::HTTP.new(uri.host, uri.port)
http.use_ssl = (uri.scheme == "https")

response = http.get(uri.request_uri)
puts("Image Status: #{response.code}")
puts("Content-Type: #{response['content-type']}")
puts("Body bytes: #{response.body&.bytesize}")
